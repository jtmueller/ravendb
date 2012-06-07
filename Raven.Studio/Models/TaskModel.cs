﻿using System.ComponentModel;
using System.Windows.Input;
using Raven.Studio.Infrastructure;

namespace Raven.Studio.Models
{
	public enum TaskStatus
	{
		DidNotStart,
		Started,
		Ended
	}

	public abstract class TaskModel : ViewModel
	{
		public TaskModel()
		{
			Output = new BindableCollection<string>(x => x);
			TaskInputs = new BindableCollection<TaskInput>(x => x.Name);
			TaskStatus = TaskStatus.DidNotStart;
		}

		private string name;
		public string Name
		{
			get { return name; }
			set { name = value; OnPropertyChanged(() => Name); }
		}

		public string Description { get; set; }

		private TaskStatus taskStatus;
		public TaskStatus TaskStatus
		{
			get { return taskStatus; }
			set
			{
				taskStatus = value;
				OnPropertyChanged(() => TaskStatus);
			}
		}

		public BindableCollection<string> Output { get; set; }
		public BindableCollection<TaskInput> TaskInputs { get; set; }

		public abstract ICommand Action { get; }
	}

	public class TaskInput : NotifyPropertyChangedBase
	{
		public TaskInput(string name, string value)
		{
			Name = name;
			Value = value;
		}

		private string name;
		public string Name
		{
			get { return name; }
			set
			{
				name = value;
				OnPropertyChanged(() => Name);
			}
		}

		private string value;
		public string Value
		{
			get { return value; }
			set { this.value = value; OnPropertyChanged(() => Value); }
		}
	}
}