using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Plugin;
using Penumbra.GameData.Structs;

namespace Penumbra.PlayerWatch
{
    public class PlayerWatcher : IPlayerWatcher
    {
        public int Version { get; } = 1;

        private static PlayerWatchBase? _playerWatch;

        public event ActorChange? ActorChanged;

        public bool Active { get; set; } = true;

        public bool Valid
            => _playerWatch != null;

        internal PlayerWatcher( DalamudPluginInterface pi )
        {
            _playerWatch ??= new PlayerWatchBase( pi );
            _playerWatch.RegisterWatcher( this );
        }

        public void Enable()
            => SetStatus( true );

        public void Disable()
            => SetStatus( false );

        public void SetStatus( bool enabled )
        {
            Active = enabled && Valid;
            _playerWatch?.CheckActiveStatus();
        }

        internal void Trigger( Actor actor )
            => ActorChanged?.Invoke( actor );

        public void Dispose()
        {
            if( _playerWatch == null )
            {
                return;
            }

            Active       = false;
            ActorChanged = null;
            _playerWatch.UnregisterWatcher( this );
            if( _playerWatch.RegisteredWatchers.Count == 0 )
            {
                _playerWatch.Dispose();
                _playerWatch = null;
            }
        }

        private void CheckValidity()
        {
            if( !Valid )
            {
                throw new Exception( $"PlayerWatch was already disposed." );
            }
        }

        public void AddPlayerToWatch( string name )
        {
            CheckValidity();
            _playerWatch!.AddPlayerToWatch( name, this );
        }

        public void RemovePlayerFromWatch( string playerName )
        {
            CheckValidity();
            _playerWatch!.RemovePlayerFromWatch( playerName, this );
        }

        public ActorEquipment UpdateActorWithoutEvent( Actor actor )
        {
            CheckValidity();
            return _playerWatch!.UpdateActorWithoutEvent( actor );
        }

        public IEnumerable< (string, ActorEquipment) > WatchedPlayers()
        {
            CheckValidity();
            return _playerWatch!.Equip
               .Where( kvp => kvp.Value.Item2.Contains( this ) )
               .Select( kvp => ( kvp.Key, kvp.Value.Item1 ) );
        }
    }

    public static class PlayerWatchFactory
    {
        public static IPlayerWatcher Create( DalamudPluginInterface pi )
            => new PlayerWatcher( pi );
    }
}