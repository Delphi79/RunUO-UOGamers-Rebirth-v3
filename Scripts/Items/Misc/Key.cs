using System;
using System.Collections; using System.Collections.Generic;
using Server.Network;
using Server.Targeting;
using Server.Prompts;
using Server.Multis;

namespace Server.Items
{
	public enum KeyType
	{
		Copper = 0x100E,
		Gold   = 0x100F,
		Iron   = 0x1010,
		Rusty  = 0x1013
	}

	public interface ILockable
	{
		bool Locked{ get; set; }
		uint KeyValue{ get; set; }
	}

	public class Key : BaseItem
	{
		private string m_Description;
		private uint m_KeyVal;
		private Item m_Link;
		private int m_MaxRange;

		public static uint RandomValue()
		{
			return (uint)(0xFFFFFFFE * Utility.RandomDouble()) + 1;
		}

		public static void RemoveKeys( Mobile m, uint keyValue )
		{
			if ( keyValue == 0 )
				return;

			Container pack = m.Backpack;

			if ( pack != null )
			{
				Item[] keys = pack.FindItemsByType( typeof( Key ), true );

				foreach ( Key key in keys )
					if ( key.KeyValue == keyValue )
						key.Delete();
			}

			BankBox box = m.BankBox;

			if ( box != null )
			{
				Item[] keys = box.FindItemsByType( typeof( Key ), true );

				foreach ( Key key in keys )
					if ( key.KeyValue == keyValue )
						key.Delete();
			}
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public string Description
		{
			get
			{
				return m_Description;
			}
			set
			{
				m_Description = value;
				InvalidateProperties();
			}
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int MaxRange
		{
			get
			{
				return m_MaxRange;
			}

			set
			{
				m_MaxRange = value;
			}
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public uint KeyValue
		{
			get
			{
				return m_KeyVal;
			}

			set
			{
				m_KeyVal = value;
				InvalidateProperties();
			}
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public Item Link
		{
			get
			{
				return m_Link;
			}

			set
			{
				m_Link = value;
			}
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 2 ); // version

			writer.Write( (int) m_MaxRange );

			writer.Write( (Item) m_Link );

			writer.Write( (string) m_Description );
			writer.Write( (uint) m_KeyVal );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			switch ( version )
			{
				case 2:
				{
					m_MaxRange = reader.ReadInt();

					goto case 1;
				}
				case 1:
				{
					m_Link = reader.ReadItem();

					goto case 0;
				}
				case 0:
				{
					if ( version < 2 || m_MaxRange == 0 )
						m_MaxRange = 3;

					m_Description = reader.ReadString();

					m_KeyVal = reader.ReadUInt();

					break;
				}
			}
		}

		[Constructable]
		public Key() : this( KeyType.Copper, 0 )
		{
		}

		[Constructable]
		public Key( KeyType type ) : this( type, 0 )
		{
		}

		[Constructable]
		public Key( uint val ) : this ( KeyType.Copper, val )
		{
		}

		[Constructable]
		public Key( KeyType type, uint LockVal ) : this( type, LockVal, null )
		{
			m_KeyVal = LockVal;
		}

		public Key( KeyType type, uint LockVal, Item link ) : base( (int)type )
		{
			Weight = 1.0;

			m_MaxRange = 3;
			m_KeyVal = LockVal;
			m_Link = link;
		}

		public Key( Serial serial ) : base( serial )
		{
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( !IsChildOf( from ) )
			{
				from.SendAsciiMessage( "That must be in your backpack to use it." );
				return;
			}

			Target t;
			int number;
			if ( m_KeyVal != 0 )
			{
				number = 501662; // What shall I use this key on?
				t = new UnlockTarget( this );
			}
			else
			{
				number = 501663; // This key is a key blank
				t = new CopyTarget( this );
			}

			from.SendLocalizedMessage( number );
			from.Target = t;
		}

		public override void GetProperties( ObjectPropertyList list )
		{
			base.GetProperties( list );

			string desc;

			if ( m_KeyVal == 0 )
				desc = "(blank)";
			else if ( (desc = m_Description) == null || (desc = desc.Trim()).Length <= 0 )
				desc = null;

			if ( desc != null )
				list.Add( desc );
		}

		public override void OnSingleClick( Mobile from )
		{
			base.OnSingleClick( from );

			string desc;

			if ( m_KeyVal == 0 )
				desc = "(blank)";
			else if ( (desc = m_Description) == null || (desc = desc.Trim()).Length <= 0 )
				desc = "";

			if ( desc.Length > 0 )
				from.Send( new AsciiMessage( Serial, ItemID, MessageType.Regular, 0x3B2, 3, "", desc ) );
		}

		private class RenamePrompt : Prompt
		{
			private Key m_Key;

			public RenamePrompt( Key key )
			{
				m_Key = key;
			}

			public override void OnResponse( Mobile from, string text )
			{
				m_Key.Description = text;
			}
		}

		public class UnlockTarget : Target
		{
			private Key m_Key;

			public UnlockTarget( Key key ) : base( key.MaxRange, false, TargetFlags.None )
			{
				m_Key = key;
				CheckLOS = false;
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				int number = -1;

				if ( targeted == m_Key )
				{
					number = 501665; // Enter a description for this key.

					from.Prompt = new RenamePrompt( m_Key );
				}
				else if ( targeted is StrongBox )
				{
					StrongBox sb = (StrongBox)targeted;

					if ( sb.Owner == null || sb.Owner.Deleted )
					{
						from.SendAsciiMessage( "You can not recover that strong box." );
						return;
					}

					if ( sb.Owner == from || sb.Owner.Account == from.Account )
					{
						while ( sb.Items.Count > 0 )
							((Item)sb.Items[0]).MoveToWorld( sb.Location, sb.Map );
						sb.Delete();
						from.AddToBackpack( new StrongBoxDeed() );
					}
					else
					{
						from.SendAsciiMessage( "You do not own that strong box." );
					}
				}
				else if ( targeted is ILockable )
				{
					ILockable o = (ILockable)targeted;

					if ( o.KeyValue == m_Key.KeyValue )
					{
						if ( o is BaseDoor && !((BaseDoor)o).UseLocks() )
						{
							number = 501668; // This key doesn't seem to unlock that.
						}
						else
						{
							o.Locked = !o.Locked;

							if ( targeted is Item )
							{
								Item item = (Item)targeted;

								if ( o.Locked )
									item.SendLocalizedMessageTo( from, 1048000 );
								else
									item.SendLocalizedMessageTo( from, 1048001 );
							}
						}
					}
					else
					{
						number = 501668; // This key doesn't seem to unlock that.
					}
				}
				else if ( targeted is HouseSign )
				{
					HouseSign sign = (HouseSign)targeted;
					if ( sign.Owner != null && sign.Owner.KeyValue == m_Key.KeyValue )
					{
						from.Prompt = new Prompts.HouseRenamePrompt( sign.Owner );
						number = 1060767; // Enter the new name of your house.
					}
				}
				else
				{
					number = 501666; // You can't unlock that!
				}

				if ( number != -1 )
				{
					from.SendLocalizedMessage( number );
				}
			}
		}

		private class CopyTarget : Target
		{
			private Key m_Key;

			public CopyTarget( Key key ) : base( 3, false, TargetFlags.None )
			{
				m_Key = key;
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				int number;

				if ( targeted is Key )
				{
					Key k = (Key)targeted;

					if ( k.m_KeyVal == 0 )
					{
						number = 501675; // This key is also blank.
					}
					else if ( from.CheckTargetSkill( SkillName.Tinkering, k, 0, 75.0 ) )
					{
						number = 501676; // You make a copy of the key.

						m_Key.Description = k.Description;
						m_Key.KeyValue = k.KeyValue;
						m_Key.Link = k.Link;
						m_Key.MaxRange = k.MaxRange;
					}
					else if ( Utility.RandomDouble() <= 0.1 ) // 10% chance to destroy the key
					{
						from.SendLocalizedMessage( 501677 ); // You fail to make a copy of the key.

						number = 501678; // The key was destroyed in the attempt.

						m_Key.Delete();
					}
					else
					{
						number = 501677; // You fail to make a copy of the key.
					}
				}
				else
				{
					number = 501688; // Not a key.
				}

				from.SendLocalizedMessage( number );
			}
		}
	}
}