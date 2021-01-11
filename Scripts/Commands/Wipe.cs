using System;
using System.Collections; using System.Collections.Generic;
using Server;
using Server.Targeting;
using Server.Items;
using Server.Multis;

namespace Server.Scripts.Commands
{
	public class Wipe
	{
		[Flags]
		public enum WipeType
		{
			Items = 0x01,
			Mobiles = 0x02,
			Multis = 0x04,
			All = Items | Mobiles | Multis
		}

		public static void Initialize()
		{
			Server.Commands.CommandSystem.Register( "Wipe", AccessLevel.GameMaster, new Server.Commands.CommandEventHandler( WipeAll_OnCommand ) );
			Server.Commands.CommandSystem.Register( "WipeItems", AccessLevel.GameMaster, new Server.Commands.CommandEventHandler( WipeItems_OnCommand ) );
			Server.Commands.CommandSystem.Register( "WipeNPCs", AccessLevel.GameMaster, new Server.Commands.CommandEventHandler( WipeNPCs_OnCommand ) );
			Server.Commands.CommandSystem.Register( "WipeMultis", AccessLevel.GameMaster, new Server.Commands.CommandEventHandler( WipeMultis_OnCommand ) );
		}

		[Usage( "Wipe" )]
		[Description( "Wipes all items and npcs in a targeted bounding box." )]
		private static void WipeAll_OnCommand( Server.Commands.CommandEventArgs e )
		{
			BeginWipe( e.Mobile, WipeType.Items | WipeType.Mobiles );
		}

		[Usage( "WipeItems" )]
		[Description( "Wipes all items in a targeted bounding box." )]
		private static void WipeItems_OnCommand( Server.Commands.CommandEventArgs e )
		{
			BeginWipe( e.Mobile, WipeType.Items );
		}

		[Usage( "WipeNPCs" )]
		[Description( "Wipes all npcs in a targeted bounding box." )]
		private static void WipeNPCs_OnCommand( Server.Commands.CommandEventArgs e )
		{
			BeginWipe( e.Mobile, WipeType.Mobiles );
		}

		[Usage( "WipeMultis" )]
		[Description( "Wipes all multis in a targeted bounding box." )]
		private static void WipeMultis_OnCommand( Server.Commands.CommandEventArgs e )
		{
			BeginWipe( e.Mobile, WipeType.Multis );
		}

		public static void BeginWipe( Mobile from, WipeType type )
		{
			BoundingBoxPicker.Begin( from, new BoundingBoxCallback( WipeBox_Callback ), type );
		}

		private static void WipeBox_Callback( Mobile from, Map map, Point3D start, Point3D end, object state )
		{
			DoWipe( from, map, start, end, (WipeType)state );
		}

		public static void DoWipe( Mobile from, Map map, Point3D start, Point3D end, WipeType type )
		{
			CommandLogging.WriteLine( from, "{0} {1} wiping from {2} to {3} in {5} ({4})", from.AccessLevel, CommandLogging.Format( from ), start, end, type, map );

			bool mobiles = ( (type & WipeType.Mobiles) != 0 );
			bool multis = ( (type & WipeType.Multis) != 0 );
			bool items = ( (type & WipeType.Items) != 0 );

			ArrayList toDelete = new ArrayList();

			Rectangle2D rect = new Rectangle2D( start.X, start.Y, end.X - start.X + 1, end.Y - start.Y + 1 );

			IPooledEnumerable eable;

			if ( (items || multis) && mobiles )
				eable = map.GetObjectsInBounds( rect );
			else if ( items || multis )
				eable = map.GetItemsInBounds( rect );
			else if ( mobiles )
				eable = map.GetMobilesInBounds( rect );
			else
				return;

			foreach ( object obj in eable )
			{
				if ( items && (obj is Item) && !((obj is BaseMulti) || (obj is HouseSign)) )
					toDelete.Add( obj );
				else if ( multis && (obj is BaseMulti) )
					toDelete.Add( obj );
				else if ( mobiles && (obj is Mobile) && !((Mobile)obj).Player )
					toDelete.Add( obj );
			}

			eable.Free();

			for ( int i = 0; i < toDelete.Count; ++i )
			{
				if ( toDelete[i] is Item )
					((Item)toDelete[i]).Delete();
				else if ( toDelete[i] is Mobile )
					((Mobile)toDelete[i]).Delete();
			}
		}
	}
}