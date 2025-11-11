using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace PWA_Restaurante.Services
{
	[Authorize]
	public class PedidosHub : Hub
	{
		public async Task JoinGroup(string groupName)
		{
			await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
		}

		public async Task LeaveGroup(string groupName)
		{
			await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
		}

		
		public async Task JoinMeserosGroup()
		{
			await JoinGroup("Meseros");
		}

		public async Task JoinCocinaGroup()
		{
			await JoinGroup("Cocina");
		}

		public async Task JoinAdminGroup()
		{
			await JoinGroup("Admin");
		}

		// Notificaciones para meseros
		public async Task NotificarPedidoListo(int pedidoId, int numMesa, DateTime horaTerminado)
		{
			await Clients.Group("Meseros").SendAsync("PedidoListo", new
			{
				pedidoId,
				numMesa,
				horaTerminado
			});
		}

		// Notificaciones para cocina
		public async Task NotificarNuevoPedido(int pedidoId, int numMesa, string estado, DateTime horaTomado)
		{
			await Clients.Group("Cocina").SendAsync("NuevoPedido", new
			{
				pedidoId,
				numMesa,
				estado,
				horaTomado
			});
		}

		// Notificación de cambio de estado
		public async Task NotificarCambioEstado(int pedidoId, string estadoAnterior, string nuevoEstado, int numMesa)
		{
			await Clients.All.SendAsync("EstadoActualizado", new
			{
				pedidoId,
				estadoAnterior,
				nuevoEstado,
				numMesa
			});
		}

		// Notificación para actualizar listas
		public async Task NotificarActualizarPedidos()
		{
			await Clients.All.SendAsync("ActualizarPedidos");
		}

		// Notificación para cocina cuando un pedido se envía
		public async Task NotificarPedidoEnviado(int pedidoId, int numMesa, DateTime horaEnviado)
		{
			await Clients.Group("Cocina").SendAsync("PedidoEnviado", new
			{
				pedidoId,
				numMesa,
				horaEnviado
			});
		}

		// Notificación cuando se cancela un pedido
		public async Task NotificarPedidoCancelado(int pedidoId, int numMesa)
		{
			await Clients.All.SendAsync("PedidoCancelado", new
			{
				pedidoId,
				numMesa
			});
		}

		public async Task NotificarListoEliminado(int pedidoId, int numMesa)
		{
			await Clients.All.SendAsync("ListoEliminado", new
			{
				pedidoId,
				numMesa
			});

			await Clients.All.SendAsync("ActualizarPedidos");
		}

		public string GetConnectionId()
		{
			return Context.ConnectionId;
		}

		public override async Task OnConnectedAsync()
		{
			await base.OnConnectedAsync();
		}

		public override async Task OnDisconnectedAsync(Exception exception)
		{
			await base.OnDisconnectedAsync(exception);
		}
	}
}
