import { Actions } from './State'

export const defaultHost = "ws://192.168.2.125:8181"

export const Command = {
  Init: 'init',
  Add: 'add',
  Update: 'update',
  Remove: 'remove'
}

let blob2str = (blob, cb) => {
  console.log(blob)
  var reader = new window.FileReader();
  reader.readAsText(blob); 
  reader.onload = function() {
    cb(JSON.parse(reader.result))
  }
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
    blob2str(msg.data, decoded => {
      console.log(decoded)
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
    })
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
