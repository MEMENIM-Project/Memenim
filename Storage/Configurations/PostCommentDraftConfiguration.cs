using System;
using Memenim.Storage.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Memenim.Storage.Configurations
{
    public class PostCommentDraftConfiguration : IEntityTypeConfiguration<PostCommentDraft>
    {
        public void Configure(EntityTypeBuilder<PostCommentDraft> builder)
        {
            builder.HasKey(draft => new { draft.UserId, draft.PostId });

            builder.Property(draft => draft.UserId)
                .ValueGeneratedNever();
            builder.Property(draft => draft.PostId)
                .ValueGeneratedNever();
            builder.Property(draft => draft.CommentText)
                .HasDefaultValue(string.Empty)
                .IsRequired();
        }
    }
}
