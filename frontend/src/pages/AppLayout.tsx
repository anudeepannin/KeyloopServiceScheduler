
import type { ReactNode } from "react";

interface AppLayoutProps {
  activePage: string;
  onNavigate: (page: string) => void;
  children: ReactNode;
}

function AppLayout({
  activePage,
  onNavigate,
  children,
}: AppLayoutProps) {
  const menuItems = [
    { id: "dashboard", label: "Dashboard", icon: "▦" },
    { id: "booking", label: "New Booking", icon: "＋" },
    { id: "appointments", label: "Appointments", icon: "▣" },
  ];

  return (
    <div className="app-layout">
      <aside className="sidebar">
        <div className="sidebar-brand">
          <div className="brand-icon">K</div>
          <div>
            <h2>Keyloop</h2>
            <p>Service Scheduler</p>
          </div>
        </div>

        <div className="sidebar-section-title">WORKSPACE</div>

        <nav className="sidebar-nav">
          {menuItems.map((item) => (
            <button
              key={item.id}
              type="button"
              className={`sidebar-link ${
                activePage === item.id ? "active" : ""
              }`}
              onClick={() => onNavigate(item.id)}
            >
              <span className="sidebar-link-icon">{item.icon}</span>
              <span>{item.label}</span>
            </button>
          ))}
        </nav>

        <div className="sidebar-footer">
          <div className="sidebar-footer-icon">●</div>
          <div>
            <strong>Service Centre</strong>
            <p>Appointment Management</p>
          </div>
        </div>
      </aside>

      <div className="app-main">
        <header className="topbar">
          <div>
            <span className="topbar-label">KEYLOOP / WORKSPACE</span>
            <p>Vehicle Service Management</p>
          </div>

          <div className="topbar-status">
            <span className="status-dot" />
            System Online
          </div>
        </header>

        <div className="page-content">{children}</div>
      </div>
    </div>
  );
}

export default AppLayout;
