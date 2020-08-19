﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParaglidingProject.Entities.Models;
using ParaglidingProject.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ParaglidingProject.Data.ContextConfiguration.ModelsConfiguration
{
    internal class ParagliderModelConfig : IEntityTypeConfiguration<ParagliderModel>
    {
        public void Configure(EntityTypeBuilder<ParagliderModel> builder)
        {
            builder.HasQueryFilter(p => p.IsActive);

            builder.Property(b => b.Size).HasMaxLength(250);
            builder.Property(b => b.ApprovalNumber).HasMaxLength(250);
            builder.Property(p => p.ApprovalDate)
                .HasColumnType("date");

           
            builder.HasMany(p => p.Paragliders)
                .WithOne(pm => pm.ParagliderModel)
                .HasForeignKey(p => p.ParagliderModelID)
                .IsRequired(true)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

