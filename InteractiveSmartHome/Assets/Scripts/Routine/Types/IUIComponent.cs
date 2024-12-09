using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public interface IUIComponent<T>
{
     UnityEvent<T> OnValueChanged { get; }

     void SetInitialValue(T value);
}