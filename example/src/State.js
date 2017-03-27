import { ValueTypes } from './TOUIValue'
import { createStore } from  'redux'

export const Actions = {
  Reset:        'reset',
  Add:          'add',
  Update:       'update',
  ClientUpdate: 'client_update',
  Remove:       'remove'
}

let defaultState = {
  values: {},
  lastAction: null
}

function handler(state = defaultState, action) {
  switch(action.type) {
  case Actions.Add:
    let values = {}
    values[action.payload.id] = action.payload
    return Object.assign({}, state, {
      values: Object.assign({}, state.values, values),
      lastAction: action
    })
  case Actions.Update:
    var value = state.values[action.payload.id]
    if(value) {
      let values = {}
      let update = Object.assign({}, value, {
        value: action.payload.value
      })
      values[action.payload.id] = update
      return Object.assign({}, state, {
        values: Object.assign({}, state.values, values),
        lastAction: action
      })
    }
    return state
  case Actions.ClientUpdate:
    var value = state.values[action.payload.id]
    if(value) {
      let values = {}
      let update = Object.assign({}, value, {
        value: action.payload.value
      })
      values[action.payload.id] = update
      return Object.assign({}, state, {
        values: Object.assign({}, state.values, values),
        lastAction: action
      })
    }
    return state
  case Actions.Remove:
    if(state.values[action.payload.id]) {
      let values = {}
      Object.keys(state.values).map(key => {
        if(key != action.payload.id)
          values[key] = state.values[key]
      })
      return Object.assign({}, state, {
        values: values,
        lastAction: action
      })
    }
    return state
  case Actions.Reset:
    return defaultState
  default:
    console.log('unknown action',action)
    return state
  }
}

export var Store = createStore(handler)
