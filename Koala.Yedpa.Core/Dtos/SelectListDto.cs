namespace Koala.Yedpa.Core.Dtos
{
    public class SelectListDto<T>
    {
        public T Val { get; set; }

        public string Key { get; set; }

        public bool IsSelected { get; set; }
    }
}
