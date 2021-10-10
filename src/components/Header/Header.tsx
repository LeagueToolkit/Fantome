import React from "react"
import { Nav } from "rsuite"

export type HeaderProps = {}

export const Header: React.FC<HeaderProps> = () => {
    return (
        <Nav appearance="tabs">
            <Nav.Item>Mod Installer</Nav.Item>
            <Nav.Item>Explorer</Nav.Item>
        </Nav>
    )
}
