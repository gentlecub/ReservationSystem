function ResourceCard({ resource, onReserve, showReserveButton = false }) {
  return (
    <div className="card shadow-sm border-0 h-100">
      <div className="card-body">
        <h5 className="card-title fw-bold">{resource.name}</h5>

        {resource.description && (
          <p className="card-text text-muted small mb-3">
            {resource.description}
          </p>
        )}

        <ul className="list-unstyled mb-0">
          {resource.location && (
            <li className="mb-2">
              <small className="text-muted">Ubicacion:</small>
              <div className="fw-semibold">{resource.location}</div>
            </li>
          )}
          {resource.capacity && (
            <li className="mb-2">
              <small className="text-muted">Capacidad:</small>
              <div className="fw-semibold">{resource.capacity} personas</div>
            </li>
          )}
        </ul>
      </div>

      {showReserveButton && (
        <div className="card-footer bg-white border-top-0 pb-3">
          <button
            className="btn btn-primary w-100"
            onClick={onReserve}
            disabled={resource.isActive === false}
          >
            {resource.isActive === false ? 'No disponible' : 'Reservar'}
          </button>
        </div>
      )}
    </div>
  )
}

export default ResourceCard
