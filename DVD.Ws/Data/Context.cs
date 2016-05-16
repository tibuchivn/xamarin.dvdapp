using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace DVD.Ws.Data
{
    public class Context : DbContext
    {
        public Context() : base("DvdConnection")
        {
            Database.SetInitializer(new NullDatabaseInitializer<Context>());
        }
        public DbSet<ImgLink> Images { get; set; }
    }

    [Table("ImgLink")]
    public class ImgLink
    {
        public int Id { get; set; }
        public string linkimg { get; set; }
        public DateTime CreateDate { get; set; }
        public bool? IsDownLoaded { get; set; }
        public string Domain { get; set; }
        public string Counter { get; set; }
        public string GroupName { get; set; }
        public string Category { get; set; }
        public bool? IsBadURL { get; set; }
        public bool? IsCheckLive { get; set; }
        public int? HotLevel { get; set; }
    }
}