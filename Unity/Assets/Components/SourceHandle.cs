using Unity.Entities;
using UnityEngine;

public struct SourceHandle : IComponentData {
	public int sourceID;
	public SourceHandle(int id){
		sourceID = id;
	}
}
