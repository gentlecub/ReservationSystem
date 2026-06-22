function ReservationCard({ reservation, onCancel, showCancelButton = false }) {
  // Obtener color del badge según estado
  const getStatusBadge = (status) => {
    switch (status) {
      case "Confirmed":
        return { color: "success", text: "Confirmada" };
      case "Pending":
        return { color: "warning", text: "Pendiente" };
      case "Cancelled":
        return { color: "danger", text: "Cancelada" };
      default:
        return { color: "secondary", text: status };
    }
  };

  // Formatear fecha
  const formatDate = (dateString) => {
    if (!dateString) return "-";
    return new Date(dateString).toLocaleDateString("es-ES", {
      weekday: "long",
      year: "numeric",
      month: "long",
      day: "numeric",
    });
  };

  const statusBadge = getStatusBadge(reservation.status);

  return (
    <div className="card shadow-sm border-0 h-100">
      <div className="card-header bg-white border-bottom-0 pt-3">
        <div className="d-flex justify-content-between align-items-start">
          <h5 className="card-title mb-0 fw-bold">
            {reservation.resourceName ||
              reservation.resource?.name ||
              "Recurso"}
          </h5>
          <span className={`badge bg-${statusBadge.color}`}>
            {statusBadge.text}
          </span>
        </div>
      </div>
      <div className="card-body pt-2">
        <ul className="list-unstyled mb-0">
          <li className="mb-2">
            <small className="text-muted">Fecha:</small>
            <div className="fw-semibold">{formatDate(reservation.date)}</div>
          </li>
          <li className="mb-2">
            <small className="text-muted">Horario:</small>
            <div className="fw-semibold">
              {reservation.startTime} - {reservation.endTime}
            </div>
          </li>
          {reservation.notes && (
            <li className="mb-2">
              <small className="text-muted">Notas:</small>
              <div className="small">{reservation.notes}</div>
            </li>
          )}
        </ul>
      </div>
      {showCancelButton && (
        <div className="card-footer bg-white border-top-0 pb-3">
          <button
            className="btn btn-outline-danger btn-sm w-100"
            onClick={onCancel}
          >
            Cancelar Reserva
          </button>
        </div>
      )}
    </div>
  );
}

export default ReservationCard;
