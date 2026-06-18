import { Outlet } from 'react-router-dom'
import '../../styles/Body.css'

function Body() {
  return (
    <div className="body-content">
      <Outlet />
    </div>
  )
}

export default Body
