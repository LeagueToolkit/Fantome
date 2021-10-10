import React from "react"
import { Container, Content, Header } from "rsuite"
import { Header as AppHeader } from "../Header"

export type LayoutProps = {}

export const Layout: React.FC<LayoutProps> = props => {
    return (
        <Container>
            <Header>
                <AppHeader />
            </Header>
            <Content>{props.children}</Content>
        </Container>
    )
}
