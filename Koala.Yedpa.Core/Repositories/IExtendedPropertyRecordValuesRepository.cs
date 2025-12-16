using Koala.Yedpa.Core.Models;

namespace Koala.Yedpa.Core.Repositories;

public interface IExtendedPropertyRecordValuesRepository
{
    Task<List<ExtendedPropertyRecordValues>> GetExtendedPropertyRecordValuesByPropertyId(string propertyId);
    Task<ExtendedPropertyRecordValues> GetExtendedPropertyRecordValuesById(string id);
    Task CreateExtendedPropertyRecordValues (ExtendedPropertyRecordValues entity);
    Task UpdateExtendedPropertyRecordValues (ExtendedPropertyRecordValues entity);
    Task DeleteExtendedPropertyRecordValues (ExtendedPropertyRecordValues entity);
}