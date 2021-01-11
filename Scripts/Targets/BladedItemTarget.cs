using System;
using Server;
using Server.Targeting;
using Server.Items;
using Server.Engines.Harvest;

namespace Server.Targets
{
	public class BladedItemTarget : Target
	{
		private Item m_Item;

		public BladedItemTarget( Item item ) : base( 2, false, TargetFlags.None )
		{
			m_Item = item;
		}

		protected override void OnTarget( Mobile from, object targeted )
		{
			if ( m_Item.Deleted )
				return;

			if ( targeted is ICarvable )
			{
				((ICarvable)targeted).Carve( from, m_Item );
			}
			else
			{
				HarvestSystem system = Lumberjacking.System;
				HarvestDefinition def = Lumberjacking.System.Definition;

				int tileID;
				Map map;
				Point3D loc;

				if ( !system.GetHarvestDetails( from, m_Item, targeted, out tileID, out map, out loc ) )
				{
					from.SendLocalizedMessage( 500494 ); // You can't use a bladed item on that!
				}
				else if ( !def.Validate( tileID ) )
				{
					from.SendLocalizedMessage( 500494 ); // You can't use a bladed item on that!
				}
				else
				{
					HarvestBank bank = def.GetBank( map, loc.X, loc.Y );

					if ( bank == null )
						return;

					if ( bank.GetCurrentFor( from ) < 5 )
					{
						from.SendLocalizedMessage( 500493 ); // There's not enough wood here to harvest.
					}
					else
					{
						bank.Consume( def, 5 );

						Item item = new Kindling();

						if ( from.PlaceInBackpack( item ) )
						{
							from.SendLocalizedMessage( 500491 ); // You put some kindling into your backpack.
							from.SendLocalizedMessage( 500492 ); // An axe would probably get you more wood.
						}
						else
						{
							from.SendLocalizedMessage( 500490 ); // You can't place any kindling into your backpack!

							item.Delete();
						}
					}
				}
			}
		}
	}
}