using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Repositories;

namespace Koala.Yedpa.Repositories.Repositories;

public class ExtendedPropertyRecordValuesRepository: IExtendedPropertyRecordValuesRepository
{
    public async Task<List<ExtendedPropertyRecordValues>> GetExtendedPropertyRecordValuesByPropertyId(string propertyId)
    {
        throw new NotImplementedException();
    }

    public async Task<ExtendedPropertyRecordValues> GetExtendedPropertyRecordValuesById(string id)
    {
        throw new NotImplementedException();
    }

    public async Task CreateExtendedPropertyRecordValues(ExtendedPropertyRecordValues entity)
    {
        throw new NotImplementedException();
    }

    public async Task UpdateExtendedPropertyRecordValues(ExtendedPropertyRecordValues entity)
    {
        throw new NotImplementedException();
    }

    public async Task DeleteExtendedPropertyRecordValues(ExtendedPropertyRecordValues entity)
    {
        throw new NotImplementedException();
    }
}