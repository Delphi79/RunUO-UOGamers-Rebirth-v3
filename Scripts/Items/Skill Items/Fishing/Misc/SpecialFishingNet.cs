using System;
using Server;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Items
{
	public class SpecialFishingNet : BaseItem
	{
		public override int LabelNumber{ get{ return 1041079; } } // a special fishing net

		private bool m_InUse;

		[Constructable]
		public SpecialFishingNet() : base( 0x0DCA )
		{
			Weight = 1.0;

			if ( 0.01 > Utility.RandomDouble() )
				Hue = Utility.RandomList( m_Hues );
			else
				Hue = 0x8A0;
		}

		private static int[] m_Hues = new int[]
			{
				0x09B,
				0x0CD,
				0x0D3,
				0x14D,
				0x1DD,
				0x1E9,
				0x1F4,
				0x373,
				0x451,
				0x47F,
				0x489,
				0x492,
				0x4B5,
				0x8AA
			};

		public SpecialFishingNet( Serial serial ) : base( serial )
		{
		}

		public override void GetProperties( ObjectPropertyList list )
		{
			base.GetProperties( list );

			// as if the name wasn't enough..
			list.Add( 1017410 ); // Special Fishing Net
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 1 ); // version

			writer.Write( m_InUse );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			switch ( version )
			{
				case 1:
				{
					m_InUse = reader.ReadBool();

					if ( m_InUse )
						Delete();

					break;
				}
			}

			Stackable = false;
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( m_InUse )
			{
				from.SendLocalizedMessage( 1010483 ); // Someone is already using that net!
			}
			else if ( IsChildOf( from.Backpack ) )
			{
				from.SendLocalizedMessage( 1010484 ); // Where do you wish to use the net?
				from.BeginTarget( -1, true, TargetFlags.None, new TargetCallback( OnTarget ) );
			}
			else
			{
				from.SendLocalizedMessage( 1042001 ); // That must be in your pack for you to use it.
			}
		}

		public void OnTarget( Mobile from, object obj )
		{
			if ( Deleted || m_InUse )
				return;

			IPoint3D p3D = obj as IPoint3D;

			if ( p3D == null )
				return;

			Map map = from.Map;

			if ( map == null || map == Map.Internal )
				return;

			int x = p3D.X, y = p3D.Y;

			if ( !from.InRange( p3D, 6 ) )
			{
				from.SendLocalizedMessage( 500976 ); // You need to be closer to the water to fish!
			}
			else if ( FullValidation( map, x, y ) )
			{
				Point3D p = new Point3D( x, y, map.GetAverageZ( x, y ) );

				for ( int i = 1; i < Amount; ++i ) // these were stackable before, doh
					from.AddToBackpack( new SpecialFishingNet() );

				m_InUse = true;
				Movable = false;
				MoveToWorld( p, map );

				from.Animate( 12, 5, 1, true, false, 0 );

				Timer.DelayCall( TimeSpan.FromSeconds( 1.5 ), TimeSpan.FromSeconds( 1.0 ), 20, new TimerStateCallback( DoEffect ), new object[]{ p, 0 } );

				from.SendLocalizedMessage( 1010487 ); // You plunge the net into the sea...
			}
			else
			{
				from.SendLocalizedMessage( 1010485 ); // You can only use this net in deep water!
			}
		}

		private void DoEffect( object state )
		{
			if ( Deleted )
				return;

			object[] states = (object[])state;

			Point3D p = (Point3D)states[0];
			int index = (int)states[1];

			states[1] = ++index;

			if ( index == 1 )
			{
				Effects.SendLocationEffect( p, Map, 0x352D, 16, 4 );
				Effects.PlaySound( p, Map, 0x364 );
			}
			else if ( index <= 10 || index == 20 )
			{
				for ( int i = 0; i < 3; ++i )
				{
					int x, y;

					switch ( Utility.Random( 8 ) )
					{
						default:
						case 0: x = -1; y = -1; break;
						case 1: x = -1; y =  0; break;
						case 2: x = -1; y = +1; break;
						case 3: x =  0; y = -1; break;
						case 4: x =  0; y = +1; break;
						case 5: x = +1; y = -1; break;
						case 6: x = +1; y =  0; break;
						case 7: x = +1; y = +1; break;
					}

					Effects.SendLocationEffect( new Point3D( p.X + x, p.Y + y, p.Z ), Map, 0x352D, 16, 4 );
				}

				Effects.PlaySound( p, Map, 0x364 );

				if ( index == 20 )
					FinishEffect( p );
				else
					this.Z -= 1;
			}
		}

		private void FinishEffect( Point3D p )
		{
			int count = Utility.RandomMinMax( 1, 3 );

			if ( Hue != 0x8A0 )
				count += Utility.RandomMinMax( 1, 2 );

			Map map = this.Map;

			for ( int i = 0; map != null && i < count; ++i )
			{
				BaseCreature spawn;

				switch ( Utility.Random( 2 ) )
				{
					default:
					case 0: spawn = new SeaSerpent(); break;
					case 1: spawn = new WaterElemental(); break;
				}

				int x = p.X, y = p.Y;

				for ( int j = 0; j < 20; ++j )
				{
					int tx = p.X - 5 + Utility.Random( 11 );
					int ty = p.Y - 5 + Utility.Random( 11 );

					LandTile t = map.Tiles.GetLandTile( tx, ty );

					if ( t.Z == p.Z && ( (t.ID >= 0xA8 && t.ID <= 0xAB) || (t.ID >= 0x136 && t.ID <= 0x137) ) && !Spells.SpellHelper.CheckMulti( new Point3D( tx, ty, p.Z ), map ) )
					{
						x = tx;
						y = ty;
						break;
					}
				}

				spawn.Map = map;
				spawn.Location = new Point3D( x, y, p.Z );
			}

			Delete();
		}

		private static int[] m_WaterTiles = new int[]
			{
				0x00A8, 0x00AB,
				0x0136, 0x0137
			};

		public static bool FullValidation( Map map, int x, int y )
		{
			bool valid = ValidateDeepWater( map, x, y );

			for ( int j = 1, offset = 5; valid && j <= 5; ++j, offset += 5 )
			{
				if ( !ValidateDeepWater( map, x + offset, y + offset ) )
					valid = false;
				else if ( !ValidateDeepWater( map, x + offset, y - offset ) )
					valid = false;
				else if ( !ValidateDeepWater( map, x - offset, y + offset ) )
					valid = false;
				else if ( !ValidateDeepWater( map, x - offset, y - offset ) )
					valid = false;
			}

			return valid;
		}

		private static bool ValidateDeepWater( Map map, int x, int y )
		{
			int tileID = map.Tiles.GetLandTile( x, y ).ID;
			bool water = false;

			for ( int i = 0; !water && i < m_WaterTiles.Length; i += 2 )
				water = ( tileID >= m_WaterTiles[i] && tileID <= m_WaterTiles[i + 1] );

			return water;
		}
	}
}