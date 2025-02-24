using Microsoft.EntityFrameworkCore;

namespace BulkyBook.DataAccess.Data;

public class HangfireDbContext(DbContextOptions<HangfireDbContext> options) : DbContext(options);   