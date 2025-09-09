public interface IDamageable
{
    void TakeDamage(float amount, UnityEngine.Vector3 sourcePosition);
    UnityEngine.Transform GetTransform();
}
