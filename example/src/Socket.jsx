import { Actions } from './State'

export const defaultHost = "ws://localhost:8181"
// export const defaultHost = "ws://91.60.81.93:40888"

export const Command = {
  Init: 'init',
  Add: 'add',
  Update: 'update',
  Remove: 'remove'
}

export const onOpen = (socket) => (
  () => { 
    console.log('intializing')
    socket.send(JSON.stringify({
        command: Command.Init
    }))
  }
)

export const onMsg = (Store) => (
  (msg) => {
    console.log("received", msg.data)
    let decoded = JSON.parse(msg.data)
    switch(decoded.command) {
      case Command.Init:
        break
      case Command.Add:
        let add = Object.assign({}, decoded.parameter.valuedefinition, {
          id: decoded.parameter.id,
          value: decoded.parameter.value
        })
        Store.dispatch({ type: Actions.Add, payload: add })
        break
      case Command.Update:
        let update = Object.assign({}, decoded.parameter.valuedefinition, {
          id: decoded.parameter.id,
          value: decoded.parameter.value
        })
        Store.dispatch({ type: Actions.Update, payload: update })
        break
      case Command.Remove:
        let remove = Object.assign({}, decoded.parameter.valuedefinition, {
          id: decoded.parameter.id,
          value: decoded.parameter.value
        })
        Store.dispatch({ type: Actions.Remove, payload: remove })
        break
      default:
        console.log('unknown command:', decoded.command)
    }
  }
)

export const onError = (Store) => (
  (err) => {
    alert("Could not connect to " + defaultHost)
    Store.dispatch({ type: Actions.Reset })
  }
) 

export const onClose = () => {
  (err) => {
    alert('Connection closed')
    Store.dispatch({ type: Actions.Reset })
  }
}
