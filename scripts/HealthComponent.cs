using Godot;
using GenericObervable;

public partial class HealthComponent : Node
{
    [Signal]
    public delegate void HPDepletedEventHandler();
    [Signal]
    public delegate void HPValueChangedEventHandler(double value);
    public Observable<double> HP {get;set;} = new();

    public HealthComponent()
    {
        HP.Value = 1;
        HP.Changed += OnHPValueChanged;
    }

    public Observable<double> GetHP()
    {
        return HP;
    }

    public void SetHP(double value)
    {
        HP.Value = value;
    }

    public void DealDamage(double damage)
    {
        HP.Value -= damage;
        if(HP.Value <= 0)
        {
            EmitSignal(SignalName.HPDepleted);
        }
    }
    void OnHPValueChanged(object target, Observable<double>.ChanedEventArgs eventArgs)
    {
        EmitSignal(SignalName.HPValueChanged, eventArgs.NewValue);
    }
}