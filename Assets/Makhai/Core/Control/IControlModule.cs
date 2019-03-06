namespace Makhai.Core.Control
{
	/// <summary>
	/// Wraps around an input or AI control, providing a unified polling interface for the two.
	/// </summary>
	public interface IControlModule
	{
		/// <summary>
		/// Polls the state of the wrapped control, returning true if the state was toggled "on" this frame.
		/// </summary>
		bool GetControlStart();

		/// <summary>
		/// Polls the state of the wrapped control, returning true if the state is currently "on".
		/// </summary>
		bool GetControlContinue();

		/// <summary>
		/// Polls the state of the wrapped control, returning true if the state was toggled "off" this frame.
		/// </summary>
		bool GetControlEnd();
	}
}
