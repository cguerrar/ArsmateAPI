// Controllers/PostController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.Sqlite;
using System.Security.Claims;
using ArsmateAPI.DTOs;
using System.Data;

namespace ArsmateAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PostsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<PostsController> _logger;
        private readonly string _connectionString;

        public PostsController(IConfiguration configuration, ILogger<PostsController> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        // POST: api/posts
        [HttpPost]
        [Authorize(Policy = "CreatorOnly")]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostDto dto)
        {
            try
            {
                // Validar entrada
                if (string.IsNullOrWhiteSpace(dto.Content) && (dto.Media == null || !dto.Media.Any()))
                {
                    return BadRequest(new { success = false, message = "Se requiere contenido o archivos multimedia" });
                }

                // Obtener usuario actual
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                // Validar tamaño total de archivos
                if (dto.Media != null && dto.Media.Any())
                {
                    long totalSize = dto.Media.Sum(m => m.Data.Length * 3 / 4); // Base64 a bytes aproximado
                    const long maxTotalSize = 50 * 1024 * 1024; // 50MB

                    if (totalSize > maxTotalSize)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = $"El tamaño total de archivos excede el límite de {maxTotalSize / 1024 / 1024}MB"
                        });
                    }
                }

                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();

                using var transaction = await connection.BeginTransactionAsync();
                try
                {
                    // Crear el post
                    var postId = Guid.NewGuid().ToString();
                    var publishedAt = dto.ScheduledAt.HasValue ? (DateTime?)null : DateTime.UtcNow;

                    var insertPostCmd = connection.CreateCommand();
                    insertPostCmd.Transaction = (SqliteTransaction?)transaction;
                    insertPostCmd.CommandText = @"
                        INSERT INTO posts (
                            id, user_id, content, visibility, price, 
                            scheduled_at, published_at, created_at, updated_at
                        ) VALUES (
                            @id, @userId, @content, @visibility, @price,
                            @scheduledAt, @publishedAt, datetime('now'), datetime('now')
                        )";

                    insertPostCmd.Parameters.AddWithValue("@id", postId);
                    insertPostCmd.Parameters.AddWithValue("@userId", userId);
                    insertPostCmd.Parameters.AddWithValue("@content", dto.Content ?? "");
                    insertPostCmd.Parameters.AddWithValue("@visibility", dto.Visibility);
                    insertPostCmd.Parameters.AddWithValue("@price", dto.Price ?? (object)DBNull.Value);
                    insertPostCmd.Parameters.AddWithValue("@scheduledAt", dto.ScheduledAt ?? (object)DBNull.Value);
                    insertPostCmd.Parameters.AddWithValue("@publishedAt", publishedAt ?? (object)DBNull.Value);

                    await insertPostCmd.ExecuteNonQueryAsync();

                    // Insertar archivos multimedia si existen
                    if (dto.Media != null && dto.Media.Any())
                    {
                        for (int i = 0; i < dto.Media.Count; i++)
                        {
                            var media = dto.Media[i];
                            var mediaId = Guid.NewGuid().ToString();

                            var insertMediaCmd = connection.CreateCommand();
                            insertMediaCmd.Transaction = (SqliteTransaction?)transaction;
                            insertMediaCmd.CommandText = @"
                                INSERT INTO post_media (
                                    id, post_id, filename, mime_type, size,
                                    data, thumbnail_data, width, height, position, created_at
                                ) VALUES (
                                    @id, @postId, @filename, @mimeType, @size,
                                    @data, @thumbnailData, @width, @height, @position, datetime('now')
                                )";

                            insertMediaCmd.Parameters.AddWithValue("@id", mediaId);
                            insertMediaCmd.Parameters.AddWithValue("@postId", postId);
                            insertMediaCmd.Parameters.AddWithValue("@filename", media.Filename);
                            insertMediaCmd.Parameters.AddWithValue("@mimeType", media.MimeType);
                            insertMediaCmd.Parameters.AddWithValue("@size", media.Size);
                            insertMediaCmd.Parameters.AddWithValue("@data", media.Data);
                            insertMediaCmd.Parameters.AddWithValue("@thumbnailData", media.ThumbnailData ?? (object)DBNull.Value);
                            insertMediaCmd.Parameters.AddWithValue("@width", media.Width ?? (object)DBNull.Value);
                            insertMediaCmd.Parameters.AddWithValue("@height", media.Height ?? (object)DBNull.Value);
                            insertMediaCmd.Parameters.AddWithValue("@position", i);

                            await insertMediaCmd.ExecuteNonQueryAsync();
                        }
                    }

                    // Insertar tags si existen
                    if (dto.Tags != null && dto.Tags.Any())
                    {
                        foreach (var tag in dto.Tags)
                        {
                            var tagId = Guid.NewGuid().ToString();
                            var insertTagCmd = connection.CreateCommand();
                            insertTagCmd.Transaction = (SqliteTransaction?)transaction;
                            insertTagCmd.CommandText = @"
                                INSERT INTO post_tags (id, post_id, tag, created_at)
                                VALUES (@id, @postId, @tag, datetime('now'))";

                            insertTagCmd.Parameters.AddWithValue("@id", tagId);
                            insertTagCmd.Parameters.AddWithValue("@postId", postId);
                            insertTagCmd.Parameters.AddWithValue("@tag", tag.ToLower());

                            await insertTagCmd.ExecuteNonQueryAsync();
                        }
                    }

                    await transaction.CommitAsync();

                    // Obtener el post creado
                    var createdPost = await GetPostByIdInternal(postId, userId, connection);

                    return CreatedAtAction(nameof(GetPost), new { id = postId }, new
                    {
                        success = true,
                        message = dto.ScheduledAt.HasValue ? "Publicación programada exitosamente" : "Publicación creada exitosamente",
                        data = createdPost
                    });
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando post");
                return StatusCode(500, new { success = false, message = "Error al crear la publicación" });
            }
        }

        // GET: api/posts/feed
        [HttpGet("feed")]
        public async Task<IActionResult> GetFeed([FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var offset = (page - 1) * limit;

                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();

                // Query para obtener posts del feed
                var query = @"
                    SELECT 
                        p.*,
                        u.username, u.display_name, u.profile_image_url, u.is_verified,
                        EXISTS(SELECT 1 FROM post_likes WHERE post_id = p.id AND user_id = @userId) as is_liked,
                        EXISTS(SELECT 1 FROM post_bookmarks WHERE post_id = p.id AND user_id = @userId) as is_bookmarked
                    FROM posts p
                    JOIN users u ON p.user_id = u.id
                    WHERE (p.published_at IS NOT NULL AND p.published_at <= datetime('now'))
                        AND (
                            p.visibility = 'public'
                            OR p.user_id = @userId
                            OR (p.visibility = 'followers' AND EXISTS(
                                SELECT 1 FROM user_follows 
                                WHERE follower_id = @userId AND following_id = p.user_id
                            ))
                            OR (p.visibility = 'subscribers' AND EXISTS(
                                SELECT 1 FROM user_subscriptions 
                                WHERE subscriber_id = @userId AND creator_id = p.user_id 
                                AND status = 'active' AND expires_at > datetime('now')
                            ))
                        )
                    ORDER BY p.published_at DESC
                    LIMIT @limit OFFSET @offset";

                var cmd = connection.CreateCommand();
                cmd.CommandText = query;
                cmd.Parameters.AddWithValue("@userId", userId ?? "");
                cmd.Parameters.AddWithValue("@limit", limit);
                cmd.Parameters.AddWithValue("@offset", offset);

                var posts = new List<PostResponseDto>();
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var post = MapPostFromReader(reader);

                    // Obtener media para cada post
                    post.Media = await GetPostMedia(post.Id, connection);

                    // Obtener tags para cada post
                    post.Tags = await GetPostTags(post.Id, connection);

                    posts.Add(post);
                }

                // Obtener total de posts
                var countCmd = connection.CreateCommand();
                countCmd.CommandText = @"
                    SELECT COUNT(*) FROM posts p
                    WHERE (p.published_at IS NOT NULL AND p.published_at <= datetime('now'))
                        AND (
                            p.visibility = 'public'
                            OR p.user_id = @userId
                            OR (p.visibility = 'followers' AND EXISTS(
                                SELECT 1 FROM user_follows 
                                WHERE follower_id = @userId AND following_id = p.user_id
                            ))
                            OR (p.visibility = 'subscribers' AND EXISTS(
                                SELECT 1 FROM user_subscriptions 
                                WHERE subscriber_id = @userId AND creator_id = p.user_id 
                                AND status = 'active' AND expires_at > datetime('now')
                            ))
                        )";
                countCmd.Parameters.AddWithValue("@userId", userId ?? "");

                var total = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        posts,
                        total,
                        hasMore = offset + limit < total,
                        page,
                        limit
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo feed");
                return StatusCode(500, new { success = false, message = "Error al obtener el feed" });
            }
        }

        // GET: api/posts/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPost(string id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();

                var post = await GetPostByIdInternal(id, userId, connection);

                if (post == null)
                {
                    return NotFound(new { success = false, message = "Post no encontrado" });
                }

                if (!post.CanView)
                {
                    return Forbid();
                }

                return Ok(new { success = true, data = post });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo post");
                return StatusCode(500, new { success = false, message = "Error al obtener el post" });
            }
        }

        // POST: api/posts/{id}/like
        [HttpPost("{id}/like")]
        public async Task<IActionResult> LikePost(string id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();

                // Verificar que puede ver el post
                if (!await CanViewPost(id, userId, connection))
                {
                    return Forbid();
                }

                var likeId = Guid.NewGuid().ToString();
                var cmd = connection.CreateCommand();
                cmd.CommandText = @"
                    INSERT OR IGNORE INTO post_likes (id, post_id, user_id, created_at)
                    VALUES (@id, @postId, @userId, datetime('now'))";
                cmd.Parameters.AddWithValue("@id", likeId);
                cmd.Parameters.AddWithValue("@postId", id);
                cmd.Parameters.AddWithValue("@userId", userId);

                await cmd.ExecuteNonQueryAsync();

                return Ok(new { success = true, message = "Like agregado" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error agregando like");
                return StatusCode(500, new { success = false, message = "Error al agregar like" });
            }
        }

        // DELETE: api/posts/{id}/like
        [HttpDelete("{id}/like")]
        public async Task<IActionResult> UnlikePost(string id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();

                var cmd = connection.CreateCommand();
                cmd.CommandText = "DELETE FROM post_likes WHERE post_id = @postId AND user_id = @userId";
                cmd.Parameters.AddWithValue("@postId", id);
                cmd.Parameters.AddWithValue("@userId", userId);

                await cmd.ExecuteNonQueryAsync();

                return Ok(new { success = true, message = "Like eliminado" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando like");
                return StatusCode(500, new { success = false, message = "Error al eliminar like" });
            }
        }

        // POST: api/posts/{id}/bookmark
        [HttpPost("{id}/bookmark")]
        public async Task<IActionResult> BookmarkPost(string id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();

                // Verificar que puede ver el post
                if (!await CanViewPost(id, userId, connection))
                {
                    return Forbid();
                }

                var bookmarkId = Guid.NewGuid().ToString();
                var cmd = connection.CreateCommand();
                cmd.CommandText = @"
                    INSERT OR IGNORE INTO post_bookmarks (id, post_id, user_id, created_at)
                    VALUES (@id, @postId, @userId, datetime('now'))";
                cmd.Parameters.AddWithValue("@id", bookmarkId);
                cmd.Parameters.AddWithValue("@postId", id);
                cmd.Parameters.AddWithValue("@userId", userId);

                await cmd.ExecuteNonQueryAsync();

                return Ok(new { success = true, message = "Post guardado en favoritos" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error guardando bookmark");
                return StatusCode(500, new { success = false, message = "Error al guardar en favoritos" });
            }
        }

        // DELETE: api/posts/{id}/bookmark
        [HttpDelete("{id}/bookmark")]
        public async Task<IActionResult> UnbookmarkPost(string id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();

                var cmd = connection.CreateCommand();
                cmd.CommandText = "DELETE FROM post_bookmarks WHERE post_id = @postId AND user_id = @userId";
                cmd.Parameters.AddWithValue("@postId", id);
                cmd.Parameters.AddWithValue("@userId", userId);

                await cmd.ExecuteNonQueryAsync();

                return Ok(new { success = true, message = "Bookmark eliminado" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando bookmark");
                return StatusCode(500, new { success = false, message = "Error al eliminar de favoritos" });
            }
        }

        // DELETE: api/posts/{id}
        [HttpDelete("{id}")]
        [Authorize(Policy = "CreatorOnly")]
        public async Task<IActionResult> DeletePost(string id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();

                // Verificar que es el autor
                var checkCmd = connection.CreateCommand();
                checkCmd.CommandText = "SELECT user_id FROM posts WHERE id = @id";
                checkCmd.Parameters.AddWithValue("@id", id);

                var postUserId = await checkCmd.ExecuteScalarAsync() as string;

                if (postUserId == null)
                {
                    return NotFound(new { success = false, message = "Post no encontrado" });
                }

                if (postUserId != userId)
                {
                    return Forbid();
                }

                // Eliminar post (cascada eliminará media, tags, likes, etc.)
                var deleteCmd = connection.CreateCommand();
                deleteCmd.CommandText = "DELETE FROM posts WHERE id = @id";
                deleteCmd.Parameters.AddWithValue("@id", id);

                await deleteCmd.ExecuteNonQueryAsync();

                return Ok(new { success = true, message = "Post eliminado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando post");
                return StatusCode(500, new { success = false, message = "Error al eliminar el post" });
            }
        }

        // Métodos auxiliares privados
        private async Task<PostResponseDto> GetPostByIdInternal(string postId, string userId, SqliteConnection connection)
        {
            var query = @"
                SELECT 
                    p.*,
                    u.username, u.display_name, u.profile_image_url, u.is_verified,
                    EXISTS(SELECT 1 FROM post_likes WHERE post_id = p.id AND user_id = @userId) as is_liked,
                    EXISTS(SELECT 1 FROM post_bookmarks WHERE post_id = p.id AND user_id = @userId) as is_bookmarked,
                    CASE 
                        WHEN p.visibility = 'public' THEN 1
                        WHEN p.user_id = @userId THEN 1
                        WHEN p.visibility = 'followers' AND EXISTS(
                            SELECT 1 FROM user_follows 
                            WHERE follower_id = @userId AND following_id = p.user_id
                        ) THEN 1
                        WHEN p.visibility = 'subscribers' AND EXISTS(
                            SELECT 1 FROM user_subscriptions 
                            WHERE subscriber_id = @userId AND creator_id = p.user_id 
                            AND status = 'active' AND expires_at > datetime('now')
                        ) THEN 1
                        ELSE 0
                    END as can_view
                FROM posts p
                JOIN users u ON p.user_id = u.id
                WHERE p.id = @postId";

            var cmd = connection.CreateCommand();
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@postId", postId);
            cmd.Parameters.AddWithValue("@userId", userId ?? "");

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var post = MapPostFromReader(reader);
                post.Media = await GetPostMedia(postId, connection);
                post.Tags = await GetPostTags(postId, connection);
                return post;
            }

            return null;
        }

        private PostResponseDto MapPostFromReader(IDataReader reader)
        {
            return new PostResponseDto
            {
                Id = reader["id"].ToString(),
                UserId = reader["user_id"].ToString(),
                Content = reader["content"].ToString(),
                Visibility = reader["visibility"].ToString(),
                Price = reader["price"] != DBNull.Value ? Convert.ToDecimal(reader["price"]) : null,
                IsPaid = Convert.ToBoolean(reader["is_paid"]),
                LikesCount = Convert.ToInt32(reader["likes_count"]),
                CommentsCount = Convert.ToInt32(reader["comments_count"]),
                SharesCount = Convert.ToInt32(reader["shares_count"]),
                ViewsCount = Convert.ToInt32(reader["views_count"]),
                ScheduledAt = reader["scheduled_at"] != DBNull.Value ? Convert.ToDateTime(reader["scheduled_at"]) : null,
                PublishedAt = reader["published_at"] != DBNull.Value ? Convert.ToDateTime(reader["published_at"]) : null,
                CreatedAt = Convert.ToDateTime(reader["created_at"]),
                UpdatedAt = Convert.ToDateTime(reader["updated_at"]),
                User = new UserInfoDto
                {
                    Id = reader["user_id"].ToString(),
                    Username = reader["username"].ToString(),
                    DisplayName = reader["display_name"].ToString(),
                    ProfileImageUrl = reader["profile_image_url"]?.ToString(),
                    IsVerified = Convert.ToBoolean(reader["is_verified"])
                },
                IsLiked = Convert.ToBoolean(reader["is_liked"]),
                IsBookmarked = Convert.ToBoolean(reader["is_bookmarked"]),
                CanView = reader.GetOrdinal("can_view") >= 0 ? Convert.ToBoolean(reader["can_view"]) : true
            };
        }

        private async Task<List<MediaDto>> GetPostMedia(string postId, SqliteConnection connection)
        {
            var mediaList = new List<MediaDto>();
            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT id, filename, mime_type, size, thumbnail_data, width, height, duration
                FROM post_media 
                WHERE post_id = @postId
                ORDER BY position";
            cmd.Parameters.AddWithValue("@postId", postId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                mediaList.Add(new MediaDto
                {
                    Id = reader["id"].ToString(),
                    Filename = reader["filename"].ToString(),
                    MimeType = reader["mime_type"].ToString(),
                    Size = Convert.ToInt32(reader["size"]),
                    ThumbnailData = reader["thumbnail_data"]?.ToString(),
                    Width = reader["width"] != DBNull.Value ? Convert.ToInt32(reader["width"]) : null,
                    Height = reader["height"] != DBNull.Value ? Convert.ToInt32(reader["height"]) : null,
                    Duration = reader["duration"] != DBNull.Value ? Convert.ToInt32(reader["duration"]) : null
                });
            }

            return mediaList;
        }

        private async Task<List<string>> GetPostTags(string postId, SqliteConnection connection)
        {
            var tags = new List<string>();
            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT tag FROM post_tags WHERE post_id = @postId";
            cmd.Parameters.AddWithValue("@postId", postId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                tags.Add(reader["tag"].ToString());
            }

            return tags;
        }

        private async Task<bool> CanViewPost(string postId, string userId, SqliteConnection connection)
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT CASE 
                    WHEN p.visibility = 'public' THEN 1
                    WHEN p.user_id = @userId THEN 1
                    WHEN p.visibility = 'followers' AND EXISTS(
                        SELECT 1 FROM user_follows 
                        WHERE follower_id = @userId AND following_id = p.user_id
                    ) THEN 1
                    WHEN p.visibility = 'subscribers' AND EXISTS(
                        SELECT 1 FROM user_subscriptions 
                        WHERE subscriber_id = @userId AND creator_id = p.user_id 
                        AND status = 'active' AND expires_at > datetime('now')
                    ) THEN 1
                    ELSE 0
                END as can_view
                FROM posts p
                WHERE p.id = @postId";

            cmd.Parameters.AddWithValue("@postId", postId);
            cmd.Parameters.AddWithValue("@userId", userId);

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToBoolean(result);
        }
    }
}