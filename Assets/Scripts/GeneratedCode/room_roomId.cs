using UnityEngine;
using System;
using System.Collections.Generic;
using ThinkEngine.Mappers;
using static ThinkEngine.Mappers.OperationContainer;
namespace ThinkEngine
{
	public class room_roomId : Sensor
	{
		private int counter;
		private object specificValue;
		private Operation operation;
		private BasicTypeMapper mapper;
		private List<List<int>> values = new List<List<int>>();
		public override void Initialize(SensorConfiguration sensorConfiguration)
		{
			this.gameObject = sensorConfiguration.gameObject;
			ready = true;
			int index = gameObject.GetInstanceID();
			mapper = (BasicTypeMapper)MapperManager.GetMapper(typeof(int));
			operation = mapper.OperationList()[0];
			counter = 0;
			mappingTemplate = "room_roomId(sensorManager,objectIndex("+index+"),{1},{0})." + Environment.NewLine;
		}
		public override void Destroy()
		{
		}
		public override void Update()
		{
			if(!ready)
			{
				return;
			}
			if(!invariant || first)
			{
				first = false;
				HospitalSensorHub HospitalSensorHub_1 = gameObject.GetComponent<HospitalSensorHub>();
				if(HospitalSensorHub_1 == null)
				{
					values.Clear();
					return;
				}
				List<RoomManager> rooms_2 = HospitalSensorHub_1.rooms;
				if(rooms_2 == null)
				{
					values.Clear();
					return;
				}
				else if(rooms_2.Count > values.Count)
				{
					for(int i = values.Count; i < rooms_2.Count; i++)
					{
						values.Add(new List<int>());
					}
				}
				else if(rooms_2.Count < values.Count)
				{
					for(int i = rooms_2.Count; i < values.Count; i++)
					{
						values.RemoveAt(values.Count - 1);
					}
				}
				for(int i_2 = 0; i_2 < rooms_2.Count; i_2++)
				{
					if(rooms_2[i_2] == null)
					{
						values[i_2].Clear();
						continue;
					}
					int roomId_3 = rooms_2[i_2].roomId;
					if (values[i_2].Count == 1)
					{
						values[i_2].RemoveAt(0);
					}
					values[i_2].Add(roomId_3);
				}
			}
		}
		public override string Map()
		{
			string mapping = string.Empty;
			for(int i0 = 0; i0 < values.Count; i0++)
			{
				object operationResult = operation(values[i0], specificValue, counter);
				if(operationResult != null)
				{
					mapping = string.Concat(mapping, string.Format(mappingTemplate, BasicTypeMapper.GetMapper(operationResult.GetType()).BasicMap(operationResult),i0));
				}
				else
				{
					mapping = string.Concat(mapping, string.Format("{0}", Environment.NewLine));
				}
			}
			return mapping;
		}
	}
}