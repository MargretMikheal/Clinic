using Microsoft.AspNet.Identity;

namespace Clinic.Core.Identity
{
    public class ApplicationRole : IRole<string>
    {
        public ApplicationRole()
        {
        }

        public ApplicationRole(string name)
        {
            Name = name;
        }

        public string Id { get; set; }
        public string Name { get; set; }
    }
}
