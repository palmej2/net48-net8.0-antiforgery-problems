using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.SystemWeb;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

namespace WebNet48
{
    public class DataProtector : DataProtectionStartup
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddDataProtection()
                .SetApplicationName("YourAppName")
                .PersistKeysToFileSystem(new DirectoryInfo(@"C:\Custom\"));
                
        }
    }
}