using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notification.Data.Model;

namespace Notification.Data.Context
{
    public class MessageConfig : IEntityTypeConfiguration<Message>
    {
        public void Configure(EntityTypeBuilder<Message> modelBuilder)
        {
            modelBuilder.ToTable("MESSAGE");

            modelBuilder.HasKey(o => o.MessageId)
            .IsClustered(true);

            modelBuilder
                .Property(s => s.MessageId)
                .HasColumnName("MESSAGE_ID")
                .UseIdentityColumn()
                .IsRequired();

            modelBuilder
                .Property(s => s.Title)
                .HasColumnName("TITLE")
                .HasMaxLength(120)
                .HasColumnType("varchar(120)")
                .IsRequired();

            modelBuilder
                .Property(s => s.Text)
                .HasColumnName("TEXT")
                .HasMaxLength(250)
                .HasColumnType("varchar(250)")
                .IsRequired();

            modelBuilder
                .Property(s => s.CreatedDate)
                .HasColumnType("timestamp")
                .HasColumnName("CREATED_DATE")
                .HasDefaultValueSql("clock_timestamp()")
                .IsRequired();

            modelBuilder
                .HasOne(b => b.Campaign)
                .WithOne(i => i.Message)
                .HasForeignKey<Campaign>(b => b.MessageId);

        }


    }
}