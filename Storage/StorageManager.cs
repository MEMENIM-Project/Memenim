using System;
using System.IO;
using System.Threading.Tasks;
using Memenim.Storage.Entities;
using Microsoft.EntityFrameworkCore;
using RIS.Logging;
using Environment = RIS.Environment;

namespace Memenim.Storage
{
    public static class StorageManager
    {
        private static StorageContext Context { get; set; }



        public static string StorageDirectoryPath { get; }
        public static string StorageFilePath { get; }

        public static bool IsInitialized { get; private set; }



        static StorageManager()
        {
            Context = null;

            StorageDirectoryPath = Path.Combine(
                Environment.ExecProcessDirectoryName,
                "storage");
            StorageFilePath = Path.Combine(
                StorageDirectoryPath,
                "storage.db");

            IsInitialized = false;
        }



        private static async Task<bool> AddPostCommentDraftInternal(
            int userId, int postId,
            string commentText, bool isAnonymous)
        {
            if (userId < 0 || postId < 0)
                return false;

            var draftTarget = new PostCommentDraft
            {
                UserId = (uint)userId,
                PostId = (uint)postId,
                CommentText = commentText,
                IsAnonymous = isAnonymous
            };

            return await AddPostCommentDraftInternal(draftTarget)
                .ConfigureAwait(false);
        }
        private static async Task<bool> AddPostCommentDraftInternal(
            PostCommentDraft draftTarget)
        {
            if (draftTarget == null)
                return false;

            await Context.AddAsync(draftTarget)
                .ConfigureAwait(false);

            await Context.SaveChangesAsync()
                .ConfigureAwait(false);

            return true;
        }

        private static async Task<bool> UpdatePostCommentDraftInternal(
            int userId, int postId,
            string commentText = null, bool? isAnonymous = null)
        {
            if (userId < 0 || postId < 0)
                return false;

            var draftTarget = await Context.PostCommentDrafts
                .FirstOrDefaultAsync(draft =>
                    draft.UserId == userId && draft.PostId == postId)
                .ConfigureAwait(false);

            return await UpdatePostCommentDraftInternal(draftTarget,
                    commentText, isAnonymous)
                .ConfigureAwait(false);
        }
        private static async Task<bool> UpdatePostCommentDraftInternal(
            PostCommentDraft draftTarget,
            string commentText = null, bool? isAnonymous = null)
        {
            if (draftTarget == null)
                return false;

            if (commentText != null)
                draftTarget.CommentText = commentText;
            if (isAnonymous != null)
                draftTarget.IsAnonymous = isAnonymous.Value;

            Context.Update(draftTarget);

            await Context.SaveChangesAsync()
                .ConfigureAwait(false);

            return true;
        }

        private static async Task<bool> RemovePostCommentDraftInternal(
            int userId, int postId)
        {
            if (userId < 0 || postId < 0)
                return false;

            var draftTarget = new PostCommentDraft
            {
                UserId = (uint)userId,
                PostId = (uint)postId
            };

            return await RemovePostCommentDraftInternal(draftTarget)
                .ConfigureAwait(false);
        }
        private static async Task<bool> RemovePostCommentDraftInternal(
            PostCommentDraft draftTarget)
        {
            if (draftTarget == null)
                return false;

            Context.Remove(draftTarget);

            await Context.SaveChangesAsync()
                .ConfigureAwait(false);

            return true;
        }



        public static async Task Initialize()
        {
            if (IsInitialized)
                return;

            Context = new StorageContext();

            await Context.Database.MigrateAsync()
                .ConfigureAwait(false);

            Context.Database.SetCommandTimeout(
                TimeSpan.FromSeconds(60));

            IsInitialized = true;
        }



        public static async Task<PostCommentDraft> GetPostCommentDraft(
            int userId, int postId)
        {
            if (!IsInitialized)
            {
                LogManager.Default.Error(
                    "Storage is not initialized");

                return new PostCommentDraft
                {
                    UserId = userId < 0
                        ? 0
                        : (uint)userId,
                    PostId = postId < 0
                        ? 0
                        : (uint)postId,
                    CommentText = string.Empty,
                    IsAnonymous = false
                };
            }

            try
            {
                if (userId < 0 || postId < 0)
                {
                    return new PostCommentDraft
                    {
                        UserId = userId < 0
                            ? 0
                            : (uint)userId,
                        PostId = postId < 0
                            ? 0
                            : (uint)postId,
                        CommentText = string.Empty,
                        IsAnonymous = false
                    };
                }

                var draftTarget = await Context.PostCommentDrafts
                    .FirstOrDefaultAsync(draft =>
                        draft.UserId == userId && draft.PostId == postId)
                    .ConfigureAwait(false);

                if (draftTarget == null)
                {
                    return new PostCommentDraft
                    {
                        UserId = (uint)userId,
                        PostId = (uint)postId,
                        CommentText = string.Empty,
                        IsAnonymous = false
                    };
                }

                return draftTarget;
            }
            catch (Exception ex)
            {
                LogManager.Default.Error(ex,
                    "Storage error");

                return new PostCommentDraft
                {
                    UserId = userId < 0
                        ? 0
                        : (uint)userId,
                    PostId = postId < 0
                        ? 0
                        : (uint)postId,
                    CommentText = string.Empty,
                    IsAnonymous = false
                };
            }
        }

        public static async Task<bool> SetPostCommentDraft(
            int userId, int postId,
            string commentText = null, bool? isAnonymous = null)
        {
            if (!IsInitialized)
            {
                LogManager.Default.Error(
                    "Storage is not initialized");

                return false;
            }

            try
            {
                if (userId < 0 || postId < 0)
                    return false;

                var draftTarget = await Context.PostCommentDrafts
                    .FirstOrDefaultAsync(draft =>
                        draft.UserId == userId && draft.PostId == postId)
                    .ConfigureAwait(false);

                if (draftTarget == null)
                {
                    if (string.IsNullOrEmpty(commentText))
                        return true;

                    return await AddPostCommentDraftInternal(userId, postId,
                            commentText, isAnonymous ?? false)
                        .ConfigureAwait(false);
                }

                if (string.IsNullOrEmpty(commentText))
                {
                    return await RemovePostCommentDraftInternal(draftTarget)
                        .ConfigureAwait(false);
                }

                return await UpdatePostCommentDraftInternal(draftTarget,
                        commentText, isAnonymous)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogManager.Default.Error(ex,
                    "Storage error");

                return false;
            }
        }
    }
}
