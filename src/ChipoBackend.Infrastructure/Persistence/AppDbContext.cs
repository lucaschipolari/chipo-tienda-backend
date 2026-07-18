using ChipoBackend.Domain.Entities.Audit;
using ChipoBackend.Domain.Entities.Catalog;
using ChipoBackend.Domain.Entities.Customers;
using ChipoBackend.Domain.Entities.Finance;
using ExpensesNs = ChipoBackend.Domain.Entities.Expenses;
using ChipoBackend.Domain.Entities.Inventory;
using ChipoBackend.Domain.Entities.Orders;
using ChipoBackend.Domain.Entities.Promotions;
using ChipoBackend.Domain.Entities.Purchasing;
using ChipoBackend.Domain.Entities.Sales;
using ChipoBackend.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace ChipoBackend.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // Auth
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    // Catalog
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductRelation> ProductRelations => Set<ProductRelation>();

    // Customers
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerAddress> CustomerAddresses => Set<CustomerAddress>();

    // Inventory
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<LostSale> LostSales => Set<LostSale>();

    // Orders
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();
    public DbSet<Payment> Payments => Set<Payment>();

    // Sales
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();

    // Purchasing
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<SupplierContact> SupplierContacts => Set<SupplierContact>();
    public DbSet<SupplierProduct> SupplierProducts => Set<SupplierProduct>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();

    // Promotions
    public DbSet<Discount> Discounts => Set<Discount>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<CouponUsage> CouponUsages => Set<CouponUsage>();
    public DbSet<Promotion> Promotions => Set<Promotion>();
    public DbSet<PromotionProduct> PromotionProducts => Set<PromotionProduct>();
    public DbSet<PromotionCategory> PromotionCategories => Set<PromotionCategory>();
    public DbSet<CouponRestriction> CouponRestrictions => Set<CouponRestriction>();

    // Finance
    public DbSet<Expense> Expenses => Set<Expense>();

    // Expenses (Gastos module)
    public DbSet<ExpensesNs.ExpenseCategory> ExpenseCategories => Set<ExpensesNs.ExpenseCategory>();
    public DbSet<ExpensesNs.Expense> GastosExpenses => Set<ExpensesNs.Expense>();

    // Audit
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // Config (clave-valor)
    public DbSet<ChipoBackend.Domain.Entities.Config.AppSetting> AppSettings => Set<ChipoBackend.Domain.Entities.Config.AppSetting>();

    // Analítica (eventos anónimos de interacción)
    public DbSet<ChipoBackend.Domain.Entities.Analytics.AnalyticsEvent> AnalyticsEvents => Set<ChipoBackend.Domain.Entities.Analytics.AnalyticsEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
