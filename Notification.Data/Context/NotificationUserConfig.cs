using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notification.Data.Model;

namespace Notification.Data.Context
{
    public class NotificationUserConfig : IEntityTypeConfiguration<NotificationUser>
    {
        public void Configure(EntityTypeBuilder<NotificationUser> modelBuilder)
        {
            //Property Configurations
            modelBuilder.ToTable("NotificationUsers");

            modelBuilder.HasKey(x => x.ID)
            .IsClustered(true);

            modelBuilder
                .Property(s => s.ID)
                .HasColumnName("ID")
                .UseIdentityColumn()
                .IsRequired();

            modelBuilder
                .Property(s => s.ConnectionId)
                .HasColumnName("CONNECTION_ID")
                .HasMaxLength(150)
                .HasColumnType("varchar(150)");

            modelBuilder
                .Property(s => s.Name)
                .HasColumnName("NAME")
                .HasMaxLength(120)
                .HasColumnType("varchar(120)");

            modelBuilder.Property(t => t.CreatedDate)
               .IsRequired()
               .HasColumnName("CREATED_DATE")
               .HasColumnType("timestamp")
               .HasDefaultValueSql("clock_timestamp()")
               ;




        }

    }
}