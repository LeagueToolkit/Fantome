import React, { useState } from 'react'
import { invoke } from '@tauri-apps/api/tauri'

import './app.css'

interface CustomResponse {
	message: string
}

export function App(): React.ReactElement {
	const [rustMsg, setRustMsg] = useState('')

	return (
		<>
			<h1>Hello World from Tauri Typescript React!</h1>
			<button onClick={() => {
				invoke('message_from_rust')
					.then((res: CustomResponse) => {
						setRustMsg(res.message)
					})
					.catch(e => {
						console.error(e)
					})
			}}>Get a Message from Rust</button>
			{!!rustMsg && (
				<h2>{rustMsg}</h2>
			)}
		</>
	)
}