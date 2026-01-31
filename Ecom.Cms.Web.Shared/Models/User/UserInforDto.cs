namespace Ecom.Cms.Web.Shared.Models.User
{
    public class UserInforDto
    {
        public UserInforDto() { }
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Avatar { get; set; }
        public string FullName { get; set; }
        public string DepartmentName { get; set; }
        public List<string> DeptCodes { get; set; }
        public string WorkplaceName { get; set; }
    }
}
