using LuciferCore.Attributes;
using LuciferCore.Main;
using Microsoft.EntityFrameworkCore;
using Server.Contract;
using Server.Database.EFcore;
using Server.Database.EFcore.Sql;
using Server.Models;

namespace Server.Core;

public class AppDI
{
    [ConsoleCommand("/init di", "Initializes the dependency injection container with the necessary services.")]
    public static void Initialize()
    {
        Lucifer.SetModelT<DbContext>(() => new EFSqlContext());

        // Register the repository
        Lucifer.SetModelT<IRepository<Account>>(() => new EFRepository<Account>());
        Lucifer.SetModelT<IRepository<Order>>(() => new EFRepository<Order>());
        Lucifer.SetModelT<IRepository<OrderItem>>(() => new EFRepository<OrderItem>());
        Lucifer.SetModelT<IRepository<Product>>(() => new EFRepository<Product>());
        Lucifer.SetModelT<IRepository<Category>>(() => new EFRepository<Category>());
        Lucifer.SetModelT<IRepository<DiscountVoucher>>(() => new EFRepository<DiscountVoucher>());

    }
}
