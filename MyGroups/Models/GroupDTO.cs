using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyGroups.Models
{
    public class GroupDTO
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string SharePointUrl { get; set; }
        public string DriveUrl { get; set; }
        public List<OneNoteDTO> Notebooks { get; set; } = new List<OneNoteDTO>();
        public List<PlanDTO> Plans { get; set; } = new List<PlanDTO>();
    }
}
