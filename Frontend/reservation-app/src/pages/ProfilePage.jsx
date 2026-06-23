import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import profileService from '../services/profileService'

function ProfilePage() {
  const { logout, updateUser } = useAuth()
  const navigate = useNavigate()

  const [profile, setProfile] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')

  // Estados para edicion
  const [editing, setEditing] = useState(false)
  const [fullName, setFullName] = useState('')
  const [phoneNumber, setPhoneNumber] = useState('')
  const [profilePhotoUrl, setProfilePhotoUrl] = useState('')
  const [saving, setSaving] = useState(false)

  // Estados para cambio de contrasena
  const [showPasswordForm, setShowPasswordForm] = useState(false)
  const [currentPassword, setCurrentPassword] = useState('')
  const [newPassword, setNewPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [changingPassword, setChangingPassword] = useState(false)

  // Estado para eliminar cuenta
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false)
  const [deleting, setDeleting] = useState(false)

  useEffect(() => {
    loadProfile()
  }, [])

  const loadProfile = async () => {
    try {
      const response = await profileService.getProfile()
      if (response.success) {
        setProfile(response.data)
        setFullName(response.data.fullName)
        setPhoneNumber(response.data.phoneNumber || '')
        setProfilePhotoUrl(response.data.profilePhotoUrl || '')
      } else {
        setError(response.message || 'Error al cargar perfil')
      }
    } catch (err) {
      setError(
        err.response?.data?.message ||
        'Error al cargar perfil'
      )
    } finally {
      setLoading(false)
    }
  }

  const handleSaveProfile = async (e) => {
    e.preventDefault()
    setError('')
    setSuccess('')
    setSaving(true)

    try {
      const response = await profileService.updateProfile({
        fullName,
        phoneNumber: phoneNumber || null,
        profilePhotoUrl: profilePhotoUrl || null
      })

      if (response.success) {
        setProfile(response.data)
        setEditing(false)
        setSuccess('Perfil actualizado exitosamente')
        updateUser({ name: fullName })
      } else {
        setError(response.message || 'Error al actualizar perfil')
      }
    } catch (err) {
      setError(
        err.response?.data?.message ||
        'Error al actualizar perfil'
      )
    } finally {
      setSaving(false)
    }
  }

  const handleChangePassword = async (e) => {
    e.preventDefault()
    setError('')
    setSuccess('')

    if (newPassword !== confirmPassword) {
      setError('Las contrasenas no coinciden')
      return
    }

    if (newPassword.length < 6) {
      setError('La nueva contrasena debe tener al menos 6 caracteres')
      return
    }

    setChangingPassword(true)

    try {
      const response = await profileService.changePassword(
        currentPassword,
        newPassword,
        confirmPassword
      )

      if (response.success) {
        setSuccess('Contrasena cambiada exitosamente')
        setShowPasswordForm(false)
        setCurrentPassword('')
        setNewPassword('')
        setConfirmPassword('')
      } else {
        setError(response.message || 'Error al cambiar contrasena')
      }
    } catch (err) {
      setError(
        err.response?.data?.message ||
        'Error al cambiar contrasena'
      )
    } finally {
      setChangingPassword(false)
    }
  }

  const handleDeleteAccount = async () => {
    setError('')
    setDeleting(true)

    try {
      const response = await profileService.deleteAccount()

      if (response.success) {
        logout()
        navigate('/login')
      } else {
        setError(response.message || 'Error al eliminar cuenta')
      }
    } catch (err) {
      setError(
        err.response?.data?.message ||
        'Error al eliminar cuenta'
      )
    } finally {
      setDeleting(false)
      setShowDeleteConfirm(false)
    }
  }

  if (loading) {
    return (
      <div className="container py-5">
        <div className="text-center">
          <div className="spinner-border text-primary" role="status">
            <span className="visually-hidden">Cargando...</span>
          </div>
        </div>
      </div>
    )
  }

  return (
    <div className="container py-4">
      <div className="row justify-content-center">
        <div className="col-md-8 col-lg-6">
          <h2 className="mb-4">Mi Perfil</h2>

          {/* Alertas */}
          {error && (
            <div className="alert alert-danger alert-dismissible fade show" role="alert">
              {error}
              <button
                type="button"
                className="btn-close"
                onClick={() => setError('')}
              ></button>
            </div>
          )}

          {success && (
            <div className="alert alert-success alert-dismissible fade show" role="alert">
              {success}
              <button
                type="button"
                className="btn-close"
                onClick={() => setSuccess('')}
              ></button>
            </div>
          )}

          {/* Informacion del perfil */}
          <div className="card shadow-sm mb-4">
            <div className="card-header d-flex justify-content-between align-items-center">
              <h5 className="mb-0">Informacion personal</h5>
              {!editing && (
                <button
                  className="btn btn-sm btn-outline-primary"
                  onClick={() => setEditing(true)}
                >
                  Editar
                </button>
              )}
            </div>
            <div className="card-body">
              {editing ? (
                <form onSubmit={handleSaveProfile}>
                  <div className="mb-3">
                    <label className="form-label">Nombre completo</label>
                    <input
                      type="text"
                      className="form-control"
                      value={fullName}
                      onChange={(e) => setFullName(e.target.value)}
                      required
                      disabled={saving}
                    />
                  </div>
                  <div className="mb-3">
                    <label className="form-label">Email</label>
                    <input
                      type="email"
                      className="form-control"
                      value={profile?.email || ''}
                      disabled
                    />
                    <div className="form-text">El email no se puede cambiar</div>
                  </div>
                  <div className="mb-3">
                    <label className="form-label">Telefono</label>
                    <input
                      type="tel"
                      className="form-control"
                      value={phoneNumber}
                      onChange={(e) => setPhoneNumber(e.target.value)}
                      placeholder="+573001234567"
                      disabled={saving}
                    />
                  </div>
                  <div className="mb-3">
                    <label className="form-label">URL de foto de perfil</label>
                    <input
                      type="url"
                      className="form-control"
                      value={profilePhotoUrl}
                      onChange={(e) => setProfilePhotoUrl(e.target.value)}
                      placeholder="https://ejemplo.com/foto.jpg"
                      disabled={saving}
                    />
                    {profilePhotoUrl && (
                      <div className="mt-2">
                        <img
                          src={profilePhotoUrl}
                          alt="Vista previa"
                          className="rounded-circle"
                          style={{ width: '60px', height: '60px', objectFit: 'cover' }}
                          onError={(e) => e.target.style.display = 'none'}
                        />
                      </div>
                    )}
                  </div>
                  <div className="d-flex gap-2">
                    <button
                      type="submit"
                      className="btn btn-primary"
                      disabled={saving}
                    >
                      {saving ? 'Guardando...' : 'Guardar'}
                    </button>
                    <button
                      type="button"
                      className="btn btn-outline-secondary"
                      onClick={() => {
                        setEditing(false)
                        setFullName(profile?.fullName || '')
                        setPhoneNumber(profile?.phoneNumber || '')
                        setProfilePhotoUrl(profile?.profilePhotoUrl || '')
                      }}
                      disabled={saving}
                    >
                      Cancelar
                    </button>
                  </div>
                </form>
              ) : (
                <>
                  {profile?.profilePhotoUrl && (
                    <div className="text-center mb-3">
                      <img
                        src={profile.profilePhotoUrl}
                        alt="Foto de perfil"
                        className="rounded-circle"
                        style={{ width: '100px', height: '100px', objectFit: 'cover' }}
                        onError={(e) => e.target.style.display = 'none'}
                      />
                    </div>
                  )}
                  <div className="mb-3">
                    <label className="form-label text-muted small">Nombre completo</label>
                    <p className="mb-0">{profile?.fullName}</p>
                  </div>
                  <div className="mb-3">
                    <label className="form-label text-muted small">Email</label>
                    <p className="mb-0">
                      {profile?.email}
                      {profile?.emailVerified ? (
                        <span className="badge bg-success ms-2">Verificado</span>
                      ) : (
                        <span className="badge bg-warning ms-2">No verificado</span>
                      )}
                    </p>
                  </div>
                  <div className="mb-3">
                    <label className="form-label text-muted small">Telefono</label>
                    <p className="mb-0">
                      {profile?.phoneNumber || 'No registrado'}
                      {profile?.phoneNumber && profile?.phoneVerified && (
                        <span className="badge bg-success ms-2">Verificado</span>
                      )}
                    </p>
                  </div>
                  <div className="mb-3">
                    <label className="form-label text-muted small">Metodo de autenticacion</label>
                    <p className="mb-0">{profile?.authProvider}</p>
                  </div>
                  <div className="mb-0">
                    <label className="form-label text-muted small">Miembro desde</label>
                    <p className="mb-0">
                      {new Date(profile?.createdAt).toLocaleDateString('es-ES', {
                        year: 'numeric',
                        month: 'long',
                        day: 'numeric'
                      })}
                    </p>
                  </div>
                </>
              )}
            </div>
          </div>

          {/* Cambiar contrasena */}
          {(profile?.authProvider === 'Local' || profile?.authProvider === 'Phone') && (
            <div className="card shadow-sm mb-4">
              <div className="card-header">
                <h5 className="mb-0">Seguridad</h5>
              </div>
              <div className="card-body">
                {showPasswordForm ? (
                  <form onSubmit={handleChangePassword}>
                    <div className="mb-3">
                      <label className="form-label">Contrasena actual</label>
                      <input
                        type="password"
                        className="form-control"
                        value={currentPassword}
                        onChange={(e) => setCurrentPassword(e.target.value)}
                        required
                        disabled={changingPassword}
                      />
                    </div>
                    <div className="mb-3">
                      <label className="form-label">Nueva contrasena</label>
                      <input
                        type="password"
                        className="form-control"
                        value={newPassword}
                        onChange={(e) => setNewPassword(e.target.value)}
                        required
                        disabled={changingPassword}
                      />
                    </div>
                    <div className="mb-3">
                      <label className="form-label">Confirmar nueva contrasena</label>
                      <input
                        type="password"
                        className="form-control"
                        value={confirmPassword}
                        onChange={(e) => setConfirmPassword(e.target.value)}
                        required
                        disabled={changingPassword}
                      />
                    </div>
                    <div className="d-flex gap-2">
                      <button
                        type="submit"
                        className="btn btn-primary"
                        disabled={changingPassword}
                      >
                        {changingPassword ? 'Cambiando...' : 'Cambiar contrasena'}
                      </button>
                      <button
                        type="button"
                        className="btn btn-outline-secondary"
                        onClick={() => {
                          setShowPasswordForm(false)
                          setCurrentPassword('')
                          setNewPassword('')
                          setConfirmPassword('')
                        }}
                        disabled={changingPassword}
                      >
                        Cancelar
                      </button>
                    </div>
                  </form>
                ) : (
                  <button
                    className="btn btn-outline-primary"
                    onClick={() => setShowPasswordForm(true)}
                  >
                    Cambiar contrasena
                  </button>
                )}
              </div>
            </div>
          )}

          {/* Zona de peligro */}
          <div className="card shadow-sm border-danger">
            <div className="card-header bg-danger text-white">
              <h5 className="mb-0">Zona de peligro</h5>
            </div>
            <div className="card-body">
              {showDeleteConfirm ? (
                <div>
                  <p className="text-danger mb-3">
                    Esta accion es irreversible. Tu cuenta sera desactivada permanentemente.
                  </p>
                  <div className="d-flex gap-2">
                    <button
                      className="btn btn-danger"
                      onClick={handleDeleteAccount}
                      disabled={deleting}
                    >
                      {deleting ? 'Eliminando...' : 'Si, eliminar mi cuenta'}
                    </button>
                    <button
                      className="btn btn-outline-secondary"
                      onClick={() => setShowDeleteConfirm(false)}
                      disabled={deleting}
                    >
                      Cancelar
                    </button>
                  </div>
                </div>
              ) : (
                <button
                  className="btn btn-outline-danger"
                  onClick={() => setShowDeleteConfirm(true)}
                >
                  Eliminar mi cuenta
                </button>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}

export default ProfilePage
