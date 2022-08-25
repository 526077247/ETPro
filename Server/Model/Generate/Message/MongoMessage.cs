using ET;
using ProtoBuf;
using System.Collections.Generic;
namespace ET
{
	[Message(MongoOpcode.ObjectQueryResponse)]
	[ProtoContract]
	public partial class ObjectQueryResponse: Object, IActorResponse
	{
		[ProtoMember(90)]
		public int RpcId { get; set; }

		[ProtoMember(91)]
		public int Error { get; set; }

		[ProtoMember(92)]
		public string Message { get; set; }

		[ProtoMember(1)]
		public Entity entity { get; set; }

	}

	[ResponseType(nameof(M2M_UnitTransferResponse))]
	[Message(MongoOpcode.M2M_UnitTransferRequest)]
	[ProtoContract]
	public partial class M2M_UnitTransferRequest: Object, IActorRequest
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public Unit Unit { get; set; }

		[ProtoMember(3)]
		public List<Entity> Entitys = new List<Entity>();

		[ProtoMember(4)]
		public List<RecursiveEntitys> Map = new List<RecursiveEntitys>();

	}

	[ResponseType(nameof(M2M_UnitAreaTransferResponse))]
	[Message(MongoOpcode.M2M_UnitAreaTransferRequest)]
	[ProtoContract]
	public partial class M2M_UnitAreaTransferRequest: Object, IActorRequest
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public Unit Unit { get; set; }

		[ProtoMember(3)]
		public List<Entity> Entitys = new List<Entity>();

		[ProtoMember(4)]
		public List<RecursiveEntitys> Map = new List<RecursiveEntitys>();

	}

	[Message(MongoOpcode.M2M_UnitAreaAdd)]
	[ProtoContract]
	public partial class M2M_UnitAreaAdd: Object, IActorLocationMessage
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public Unit Unit { get; set; }

		[ProtoMember(3)]
		public List<Entity> Entitys = new List<Entity>();

		[ProtoMember(4)]
		public List<RecursiveEntitys> Map = new List<RecursiveEntitys>();

		[ProtoMember(5)]
		public MoveInfo MoveInfo { get; set; }

	}

	[Message(MongoOpcode.M2M_UnitAreaCreate)]
	[ProtoContract]
	public partial class M2M_UnitAreaCreate: Object, IActorLocationMessage
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public Unit Unit { get; set; }

		[ProtoMember(3)]
		public List<Entity> Entitys = new List<Entity>();

		[ProtoMember(4)]
		public List<RecursiveEntitys> Map = new List<RecursiveEntitys>();

	}

	[Message(MongoOpcode.RecursiveEntitys)]
	[ProtoContract]
	public partial class RecursiveEntitys: Object
	{
		[ProtoMember(1)]
		public int IsChild { get; set; }

		[ProtoMember(2)]
		public int ParentIndex { get; set; }

		[ProtoMember(3)]
		public int ChildIndex { get; set; }

	}

}
