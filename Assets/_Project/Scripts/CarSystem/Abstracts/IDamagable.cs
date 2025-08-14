
public interface IDamagable
{
    public int Health { get; set; }
    public int Damage { get; }
    public void TakeDamage(int damage);
}
