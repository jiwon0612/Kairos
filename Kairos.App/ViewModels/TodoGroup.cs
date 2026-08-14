using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kairos.App.ViewModels
{
    public class TodoGroup : ObservableCollection<TodoViewModel>
    {
        public string ProjectName { get; }

        public TodoGroup(string projectName, IEnumerable<TodoViewModel> todos) : base(todos)
        {
            ProjectName = projectName;
        }
    }
}
