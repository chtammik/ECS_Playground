using Unity.Entities;
using UnityEngine;

public struct Clip : IComponentData {
	public int id;
	public float length;

	public Clip(AudioClip clip){
		id = clip.GetInstanceID();
		length = clip.length;
	}
}
