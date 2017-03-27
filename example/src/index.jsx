import React from 'react'
import { render } from 'react-dom'
import App from './App'
import { ValueTypes } from './TOUIValue'
import { Store, Actions } from './State'
import { Provider } from 'react-redux'
import * as Socket from './Socket'

const examples = [{
  Id: "037e7097-41eb-46a6-9ef0-ac1234fd84ff",
  Type: ValueTypes.Boolean,
  Default: false,
  Value: true,
  Label: "Bool Example",
  Description: "This is a sample TOUI bool value",
  UserData: "ohai"
}, {
  Id: "df462b02-8607-407c-b308-d13d344e6e65",
  Type: ValueTypes.String,
  Default: "",
  Value: "/path/to/something",
  Label: "String Example",
  Description: "This is a sample TOUI string value",
  UserData: null
}, {
  Id: "9513aa66-76b0-4a1e-b295-9bf26a8ce37a",
  Type: ValueTypes.Number,
  Default: 0.0,
  Value: 234,
  Label: "Number Example",
  Description: "This is a sample TOUI number value",
  UserData: null
}, {
  Id: "e2032f16-d3a3-48a4-8165-4f991380cafd",
  Type: ValueTypes.Color,
  Default: 0.0,
  Value: 234,
  Label: "Color Example",
  Description: "This is a sample TOUI color value",
  UserData: null
}, {
  Id: "3568a396-3e55-4969-bc7c-6b8748aef63e",
  Type: ValueTypes.Enum,
  Default: 0.0,
  Value: 234,
  Label: "Enum Example",
  Description: "This is a sample TOUI enum value",
  UserData: null
}];

window.onload = () => {
  render(
    <Provider store={Store}>
      <App/>
    </Provider>,
    document.getElementById('main'));

  let ws = new WebSocket(Socket.defaultHost)

  ws.onopen = Socket.onOpen(ws)
  ws.onmessage = Socket.onMsg(Store)
  ws.onclose = Socket.onClose(Store)
  ws.onerror = Socket.onError(Store)

  Store.subscribe(() => {
    let value = Store.getState().lastAction
    if (value && value.type === Actions.ClientUpdate) {
      let data = JSON.stringify({
        command: Socket.Command.Update,
        parameter: {
          id: value.payload.id,
          value: value.payload.value
        }
      })
      console.log('sending', data)
      ws.send(data)
    }
  })

  window.sock = ws;
};
