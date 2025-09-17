using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Effectable : MonoBehaviour, IAffectable
{
    private List<IEffect> _activeEffects = new List<IEffect>();

    void Update()
    {
        foreach (var effect in _activeEffects.ToList())
        {
            effect.Tick(this);
            if (effect.IsFinished)
            {
                RemoveEffect(effect);
            }
        }
    }

    public void AddEffect(IEffect effect)
    {
        // TODO: Đây là nơi chúng ta sẽ xử lý logic kết hợp nguyên tố.
        // Ví dụ, nếu thêm BurnEffect vào mục tiêu đã có BurnEffect,
        // thì xóa cả 2 và tạo hiệu ứng Explosion.
        
        _activeEffects.Add(effect);
        effect.OnAttached(this);
        Debug.Log($"Applied effect {effect.GetType().Name} to {gameObject.name}");
    }

    private void RemoveEffect(IEffect effect)
    {
        if (_activeEffects.Contains(effect))
        {
            effect.OnDetached(this);
            _activeEffects.Remove(effect);
            Debug.Log($"Removed effect {effect.GetType().Name} from {gameObject.name}");
        }
    }

    // check if has effect of type T
    public bool HasEffectOfType<T>() where T : IEffect
    {
        return _activeEffects.Any(e => e is T);
    }
}