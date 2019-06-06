# efcore-separate-migrations

## 概要

EF CoreのMigrationsファイルをDbContextのあるライブラリとは別で管理する構成です。

## 構成

![image](https://raw.githubusercontent.com/ushibutatory/efcore-separate-migrations/master/docs/images/solution_explorer.png)

### Sample.Data

- クラスライブラリ
- .NET Standard 2.0
- NuGet:
    - Microsoft.EntityFrameworkCore
    - Microsoft.EntityFrameworkCore.SqlServer（任意）
        - 使用するDBにあわせて適宜変更します。
- DbContextやEntityクラスはここで定義します。

### Sample.Data.Operator

- コンソールアプリケーション
- .NET Core 2.2
- NuGet:
    - Microsoft.EntityFrameworkCore.Design
        - `add-migration`、`update-database`などの実行に必要です。
    - Microsoft.Extensions.CommandLineUtils（任意）
        - Migration実行時以外、単体で実行された場合の挙動を制御するために使用しています。
    - Microsoft.Extensions.Configuration.Json（任意）
        - 接続文字列を管理するために使用しています。
- Migrationファイルはここに作成されていきます。

ASP.NETアプリケーション、コンソールアプリケーションなどを開発する場合は、Sample.Dataを参照追加します。

こうすることで、Webアプリケーション等がMigration関連のパッケージを読み込む必要がなくなります。

## 解説

### Sample.Data

DbContext、Entityを普通に作成します。

### Sample.Data.Operator

`IDesignTimeDbContextFactory` を継承したクラスを作成します。

- IDesignTimeDbContextFactoryについて
    - https://docs.microsoft.com/en-us/ef/core/miscellaneous/cli/dbcontext-creation

その際、 `MigrationsAssembly()` でMigrationファイルの作成先を指定します。

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Sample.Data.Operator
{
    public class DbContextFactory : IDesignTimeDbContextFactory<DbContext>
    {
        public DbContext CreateDbContext(string[] args)
        {
            // NOTE: appsettings.jsonから読み込む場合
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .Build();
            var connectionString = config.GetConnectionString("Default");

            // NOTE: べた書きする場合
            //var connectionString = "Server=...";

            // NOTE: MigrationsAssemblyでMigrationファイルを作成するアセンブリを指定する
            var builder = new DbContextOptionsBuilder<DbContext>()
                .UseSqlServer(connectionString, _ => _.MigrationsAssembly(typeof(DbContextFactory).Namespace));

            return new DbContext(builder.Options);
        }
    }
}
```

`Program.cs` は特に必要ありません。

### マイグレーションの作成・実行

#### Visual Studio パッケージマネージャコンソール上で実行する場合

- [既定のプロジェクト]は `Sample.Data.Operator` を選択する
- `Sample.Data.Operator` をスタートアッププロジェクトに設定する

上記の状態で各コマンドを実行します。

```cmd
PM> add-migration init
To undo this action, use Remove-Migration.

PM> update-database
Applying migration '...`.
Done.
```

#### CLIで実行する場合

参考）
https://docs.microsoft.com/en-us/ef/core/miscellaneous/cli/dotnet

```cmd
$ cd .../Sample.Data.Operator

$ dotnet ef migrations add init
Done. To undo this action, use 'ef migrations remove'

$ dotnet ef database update
Applying migration '...'.
Done.
```
