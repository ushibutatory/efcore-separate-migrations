using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.CommandLineUtils;
using Sample.Data.Entities;
using System;
using System.Linq;

namespace Sample.Data.Operator
{
    public class Program
    {
        public static int Main(string[] args)
        {
            // NOTE: 本筋から逸れるが、初期データ投入などの作業をコマンドラインでできるようにしてみた
            var app = new CommandLineApplication(throwOnUnexpectedArg: false)
            {
                Name = "Sample.Data.Operator",
            };

            app.HelpOption("-h|--help");

            // NOTE: 複雑になってきたら別クラスに分けるなどを検討する
            app.Command("init", _ =>
            {
                _.Description = "データを初期化します。";

                _.OnExecute(async () =>
                {
                    using (var dbContext = new DbContextFactory().CreateDbContext(args))
                    {
                        // データクリア
                        app.Execute("clear");

                        // 初期データ作成
                        var dogs = new[]
                        {
                            new Dog { Name = "Pochi" },
                            new Dog { Name = "Mike" },
                            new Dog { Name = "Tama" },
                            new Dog { Name = "Shiro" },
                            new Dog { Name = "Kuro" }
                        };

                        await dbContext.Dogs.AddRangeAsync(dogs);
                        await dbContext.SaveChangesAsync();

                        Console.WriteLine($"{dogs.Count()} dogs came.{{{string.Join(", ", dogs.Select(dog => dog.Name))}}}");

                        return 0;
                    }
                });
            });

            app.Command("clear", _ =>
            {
                _.Description = "データをクリアします。";

                _.OnExecute(async () =>
                {
                    using (var dbContext = new DbContextFactory().CreateDbContext(args))
                    {
                        var dogs = await dbContext.Dogs.ToListAsync();
                        if (dogs?.Count > 0)
                        {
                            var dogCount = dogs.Count;

                            dogs.ForEach(dog => dbContext.Dogs.Remove(dog));
                            await dbContext.SaveChangesAsync();

                            Console.WriteLine($"{dogCount} dogs are free.");
                        }

                        return 0;
                    }
                });
            });

            app.Command("list", _ =>
            {
                _.Description = "一覧表示します。";

                _.OnExecute(async () =>
                {
                    using (var dbContext = new DbContextFactory().CreateDbContext(args))
                    {
                        var dogs = await dbContext.Dogs.ToListAsync();

                        var text = (dogs?.Count == 0)
                            ? $"There are no dogs."
                            : $"There are {dogs.Count()} dogs. {{{string.Join(", ", dogs.Select(dog => dog.Name))}}}";

                        Console.WriteLine(text);

                        return 0;
                    }
                });
            });

            // 引数なしで実行された場合はヘルプ表示
            if (args?.Length == 0)
                args = new[] { "-h" };

            return app.Execute(args);
        }
    }
}
