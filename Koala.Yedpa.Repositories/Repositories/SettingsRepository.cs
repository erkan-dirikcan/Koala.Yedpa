using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Koala.Yedpa.Repositories.Repositories
{
    public class SettingsRepository(AppDbContext context) : ISettingsRepository
    {
        private readonly DbSet<Settings> _dbSet = context.Set<Settings>();

        public async Task<List<Settings>> GetSettings(SettingsTypeEnum type)
        {
            var res = await _dbSet.Where(x => x.SettingType == type).ToListAsync();
            return res;
        }

        public async Task<Settings?> GetSetting(string id)
        {
            var res = await _dbSet.FirstOrDefaultAsync(x => x.Id == id);
            return res;
        }

        public async Task<Settings?> GetSettingByName(string name, SettingsTypeEnum type)
        {
            var res = await _dbSet.FirstOrDefaultAsync(x => x.Name == name && x.SettingType == type);
            return res;
        }
        public async Task<List<Settings>> GetSettingsByNames(IEnumerable<string> names, SettingsTypeEnum type)
        {
            return await _dbSet
                .Where(x => names.Contains(x.Name) && x.SettingType == type)
                .ToListAsync();
        }
        public void UpdateSettings(Settings settings)
        {
            context.Entry(settings).State = EntityState.Modified;
        }

        public async Task AddSettingsAsync(List<Settings> settings)
        {
            foreach (var t in settings)
            {
                await context.AddAsync(t);
            }
        }
        public async Task AddSettingsAsync(Settings setting)
        {
            await context.AddAsync(setting);

        }
    }
}
