using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace EF_Update_To_Delete_Bug
{
    public class Blog
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int BlogId { get; set; }
        public Blog Blog { get; set; }
    }

    public class TestContext : DbContext
    {
        public TestContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }
    }

    static class Program
    {
        static void Main(string[] args)
        {
            // Setup db and test data.
            var optBuilder = new DbContextOptionsBuilder();
            optBuilder.UseSqlServer(
                "data source=.;initial catalog=TestDb;integrated security=True;MultipleActiveResultSets=True;App=EntityFramework");

            int id = 0;
            using (var ctx = new TestContext(optBuilder.Options))
            {
                ctx.Database.EnsureDeleted();
                ctx.Database.EnsureCreated();

                var blog = new Blog{ Name = "The Blog" };
                var post = new Post{ Title = "The Post", Blog = blog };

                ctx.Blogs.Add(blog);
                ctx.Posts.Add(post);
                ctx.SaveChanges();
                id = post.Id;
            }

            // Update entity.
            using (var ctx = new TestContext(optBuilder.Options))
            {
                var post = ctx.Posts.Find(id);
                ctx.Entry(post).Reference(x => x.Blog).Load();

                post.Blog = null;
                post.Title = "New title.";

                ctx.SaveChanges();
            }


            using (var ctx = new TestContext(optBuilder.Options))
            {
                var child = ctx.Posts.Find(id);
                Debug.Assert(child != null, "Post should not be deleted because Blog navigation property is set to null.");
            }
        }
    }
}
