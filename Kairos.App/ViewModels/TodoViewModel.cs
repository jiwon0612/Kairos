using Kairos.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kairos.App.ViewModels
{
    public class TodoViewModel
    {
        private readonly TodoResponse _todo;

        public TodoViewModel(TodoResponse todo)
        {
            _todo = todo;
        }

        public int ID => _todo.ID;
        public string Title => _todo.Title;
        public bool IsCompleted
        {
            get => _todo.IsCompleted;
            set => _todo.IsCompleted = value;
        }
        public int Priority => _todo.Priority;
        public DateTime? DueDate => _todo.DueDate;
        public bool HasDueTime => _todo.HasDueTime;

        public string DueDateText
        {
            get
            {
                if (_todo.DueDate == null)
                    return "";

                var due = _todo.DueDate.Value.ToLocalTime();
                var today = DateTime.Today;
                int days = (due - today).Days;

                if (_todo.HasDueTime)
                {
                    string dayPart = days switch
                    {
                        < 0 => "지남",
                        0 => "오늘",
                        1 => "내일",
                        _ => $"{due.Month}/{due.Day}"
                    };
                    return $"{dayPart} {due:HH:mm}";
                }
                else
                {
                    return days switch
                    {
                        < 0 => "지남",
                        0 => "오늘까지",
                        1 => "내일까지",
                        _ => $"{due.Month}/{due.Day}까지"
                    };
                }
            }
        }

        public Color DueDateColor
        {
            get
            {
                if (_todo.DueDate == null)
                    return Colors.Transparent;

                var due = _todo.DueDate.Value.ToLocalTime().Date;
                int days = (due - DateTime.Today).Days;

                return days switch
                {
                    < 0 => Colors.IndianRed,   
                    0 => Colors.IndianRed,     
                    1 => Colors.Goldenrod,    
                    _ => Colors.Gray
                };
            }
        }
    }
}
