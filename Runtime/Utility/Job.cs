using System;

namespace Shared.Foundation
{
	public enum JobState
	{
		Idle,
		Running,
		Complete
	}

	public abstract class Job
	{
		public Action<Job> OnStart;
		public Action<Job> OnCompleted;
		public Action<Job, float> OnProgressed;

		public virtual float Weight => 1f;
		protected virtual bool CompleteAfterExecution => true;
		public virtual JobState State { get; protected set; } = JobState.Idle;

		private float _progress;
		public virtual float Progress
		{
			get => _progress;
			protected set
			{
				_progress = value;
				OnProgressed?.Invoke(this, _progress);
			}
		}

		public virtual void Start()
		{
			Progress = 0;
			State = JobState.Running;
			OnStart?.Invoke(this);

			Execute();

			if (CompleteAfterExecution)
				Complete();
		}

		protected void Complete()
		{
			Progress = 1;
			State = JobState.Complete;
			OnCompleted?.Invoke(this);
		}

		protected abstract void Execute();
	}
}