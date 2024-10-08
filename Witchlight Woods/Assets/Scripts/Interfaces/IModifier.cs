using System;

namespace WitchlightWoods
{
    public interface IModifier<T>
    {
        public bool ShouldRemove();
        public T Modify(T input);
        public void Update(float deltaTime);
    }
}